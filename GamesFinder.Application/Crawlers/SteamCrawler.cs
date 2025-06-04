using System.Net;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Crawlers;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GamesFinder.Application.Crawlers;

public class SteamCrawler : Crawler, ICrawler
{
    private static readonly int MaxReqs = 200;
    private static readonly int CooldownMinutes = 5;
    private static readonly string PackageDetails = "https://store.steampowered.com/api/packagedetails/?packageids=";

    public SteamCrawler(ILogger<SteamCrawler> logger, IGameRepository<Game> gameRepository, IGameOfferRepository<GameOffer> gameOfferRepository) :
        base(
            gameData: "https://store.steampowered.com/api/appdetails?appids=",
            gameRepository: gameRepository,
            gameOfferRepository: gameOfferRepository,
            logger: logger
            )
    {
        
    }

    public override async Task CrawlGamesAsync(ICollection<int>? gameIds, bool force = false)
    {
        if (gameIds == null)
        {
            Logger.LogCritical("Steam requires gameIds!");
            return;
        }
        
        var games = new List<Game>();

        foreach (int gameId in gameIds)
        {
            Logger.LogInformation($"Crawling {gameId}");
            if (!force)
            {
                if (await GameRepository.ExistsByAppIdAsync(gameId)) continue;
            }

            var constructedUrl = GameData + gameId + "&l=ru";
            var json = await MakeRequest(constructedUrl, true, gameId);
            if (json == null)
            {
                await SaveOrUpdateBulk(games); //If error save what processed
                continue;
            }
            
            var (game, offer, isNewGame) = await ExtractData(content: json, url: constructedUrl, appId: gameId, ignorePackages: false, forcePackageUpdate: force);
            if (game == null) continue;
            
            games.Add(game);
        }
        
        await SaveOrUpdateBulk(games);
    }
    
    
    public override async Task CrawlPricesAsync(ICollection<Game>? games, bool force = false)
    {
        var callsCount = 0;
        throw new NotImplementedException("Implement");
    }

    protected override async Task<(Game?, GameOffer?, bool)> ExtractData(string content, string url, int? appId = null, Game? existingGame = null, bool ignorePackages = true, bool forcePackageUpdate = false)
    {
        var jObj = JsonConvert.DeserializeObject<JObject>(content)?[$"{appId}"];
        if (jObj == null)
        {
            Logger.LogError($"Failed to parse json: {content}");
            return (null, null, false);
        }

        string? name = jObj["data"]?["name"]?.ToString();
        string? description = jObj["data"]?["detailed_description"]?.ToString();
        string? currency = jObj["data"]?["price_overview"]?["currency"]?.ToString();
        string initialPrice = jObj["data"]?["price_overview"]?["initial"]?.ToString() ?? "0";
        string? currentPrice = jObj["data"]?["price_overview"]?["final"]?.ToString();
        string? thumbnail = jObj["data"]?["header_image"]?.ToString();
        string steamUrl = $"https://store.steampowered.com/app/{appId}";
        string? packages = jObj["data"]?["packages"]?.ToString();

        if (name == null) return (null, null, false);
        Game game = existingGame ?? new Game(name: name, description: description, steamUrl: steamUrl, headerImage: thumbnail);
        game.GameIds.Add(new Game.GameId(EVendor.Steam, appId.ToString()));

        if (packages != null && !ignorePackages)
        {
            await GetPackageDetails(packages, forcePackageUpdate);
        }

        Dictionary<ECurrency, GameOffer.PriceRange> prices = new();
        switch (currency)
        {
            case null:
                prices.Add(ECurrency.Eur, new GameOffer.PriceRange(null, null));
                break;
            case "EUR":
                prices.Add(ECurrency.Eur, new GameOffer.PriceRange(decimal.Parse(initialPrice), decimal.Parse(currentPrice ?? initialPrice)));
                break;
            case "USD":
                prices.Add(ECurrency.Usd, new GameOffer.PriceRange(decimal.Parse(initialPrice), decimal.Parse(currentPrice ?? initialPrice)));
                break;
        }

        var offer = new GameOffer(
            gameId: game.Id!,
            vendor: EVendor.Steam,
            vendorsUrl: url,
            prices: prices,
            available: true);
        
        game.Offers.Add(offer);
        
        return (game, offer, true);
    }

    private async Task<string?> MakeRequest(string url, bool isGame, int id)
    {
        //TODO: Use proxy? Open proxy = faster, but less safe :(
        var response = await Client.GetAsync(new Uri(url));
        if (!response.IsSuccessStatusCode)
        {
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                Logger.LogWarning($"Process is paused for {CooldownMinutes} minutes");
                await Task.Delay(TimeSpan.FromMinutes(CooldownMinutes));
                response = await Client.GetAsync(new Uri(url));
            }
            else
            {
                var target = isGame ? "game data" : "package data";
                Logger.LogError($"Error getting {target} for {id}, {response.StatusCode}");
                return null;
            }
        }

        try
        {
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception e)
        {
            Logger.LogCritical($"Could not parse content! ${e.Message}");
            return null;
        }
    }

    private async Task GetPackageDetails(string packages, bool force)
    {
        var parsedData = packages.Replace("[", "").Replace("]", "").Replace("\r", "").Replace("\n", "").Replace(" ", "");
        var asList = parsedData.Split(',').ToList();
        var games = new List<Game>();
        
        foreach (var packageId in asList)
        {
            int asInt = int.Parse(packageId);
            if (!force)
            {
                if (await GameRepository.ExistsByAppIdAsync(asInt)) continue;
            }

            var constructedUrl = PackageDetails + packageId;
            
            var json = await MakeRequest(constructedUrl, false, asInt);
            if (json == null) continue;
            
            var (game, offer, isNewGame) = await ExtractData(content: json, url: constructedUrl, appId: asInt, ignorePackages: true, forcePackageUpdate: force);
            if (game == null) continue;
            
            games.Add(game);
        }
        
        await SaveOrUpdateBulk(games);
    }
}