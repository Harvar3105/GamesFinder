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
        var callsCount = 0;

        foreach (int gameId in gameIds)
        {
            if (callsCount != 0 && callsCount % MaxReqs == 0)
            {
                await SaveOrUpdateBulk(games);
                await Task.Delay(TimeSpan.FromMinutes(CooldownMinutes));
            }

            if (!force)
            {
                if (await GameRepository.ExistsByAppIdAsync(gameId)) continue;
            }

            var constructedUrl = GameData + gameId + "&l=ru";
            
            var response = await Client.GetAsync(new Uri(constructedUrl));
            callsCount++;
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError($"Error getting game data for {gameId}, ${response.StatusCode}");
                continue;
            }
            
            var json = await response.Content.ReadAsStringAsync();
            
            var (game, offer, isNewGame) = await ExtractData(content: json, url: constructedUrl, appId: gameId);
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

    protected override Task<(Game?, GameOffer?, bool)> ExtractData(string content, string url, int? appId = null, Game? existingGame = null)
    {
        var jObj = JsonConvert.DeserializeObject<JObject>(content)?[$"{appId}"];
        if (jObj == null)
        {
            Logger.LogError($"Failed to parse json: {content}");
            return Task.FromResult<(Game?, GameOffer?, bool)>((null, null, false));
        }

        string? name = jObj["data"]?["name"]?.ToString();
        string? description = jObj["data"]?["detailed_description"]?.ToString();
        string? currency = jObj["data"]?["price_overview"]?["currency"]?.ToString();
        string? initialPrice = jObj["data"]?["price_overview"]?["initial"]?.ToString() ?? "0";
        string? currentPrice = jObj["data"]?["price_overview"]?["final"]?.ToString();
        string? thumbnail = jObj["data"]?["header_image"]?.ToString();
        string steamUrl = $"https://store.steampowered.com/app/{appId}";

        if (name == null) return Task.FromResult<(Game?, GameOffer?, bool)>((null, null, false));
        Game game = existingGame ?? new Game(name: name, description: description, steamUrl: steamUrl, headerImage: thumbnail);
        game.GameIds.Add(new Game.GameId(EVendor.Steam, appId.ToString()));

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
        
        return Task.FromResult<(Game?, GameOffer?, bool)>((game, offer, true));
    }
}