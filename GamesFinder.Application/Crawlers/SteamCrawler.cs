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
            Game? game = await GameRepository.GetByAppId(gameId);

            if (force || game == null)
            {
                game = await GetNewGame(gameId);
            }
            else
            {
                game = await UpdateGamePrice(game);
            }
            
            if (game == null)
            {
                Logger.LogError($"Could not find game {gameId}");
                await SaveOrUpdateBulk(games);
                continue;
            }
            games.Add(game);
        }
        
        await SaveOrUpdateBulk(games);
    }

    private string? ExtractName(JToken jObj)
    {
        return jObj["data"]?["name"]?.ToString();
    }

    private EType ExtractType(JToken jObj)
    {
        if (jObj["data"]?["type"] != null)
        {
            return EType.DLC;
        }

        return EType.Game;
    }

    private string? ExtractDescription(JToken jObj)
    {
        return jObj["data"]?["about_the_game"]?.ToString();
    }

    private Dictionary<ECurrency, GameOffer.PriceRange> ExtractPrices(JToken jObj)
    {
        string? currency = jObj["data"]?["price_overview"]?["currency"]?.ToString();
        
        string? initialPriceAsString = jObj["data"]?["price_overview"]?["initial"]?.ToString();
        string? currentPriceAsString = jObj["data"]?["price_overview"]?["final"]?.ToString();
        
        decimal? initialPrice = initialPriceAsString != null ? decimal.Parse(initialPriceAsString) / 100 : null;
        decimal? currentPrice = currentPriceAsString != null ? decimal.Parse(currentPriceAsString) / 100 : null;
        
        Dictionary<ECurrency, GameOffer.PriceRange> prices = new();
        
        switch (currency)
        {
            case null:
                prices.Add(ECurrency.Eur, new GameOffer.PriceRange(null, null));
                break;
            case "EUR":
                prices.Add(ECurrency.Eur, new GameOffer.PriceRange(initialPrice, currentPrice));
                break;
            case "USD":
                prices.Add(ECurrency.Usd, new GameOffer.PriceRange(initialPrice, currentPrice));
                break;
        }
        
        return prices;
    }

    private string? ExtractThumbnail(JToken jObj)
    {
        return jObj["data"]?["header_image"]?.ToString();
    }

    private GameOffer ExtractGameOffer(JToken jObj, Game game, string vendorsGameId)
    {
        return new GameOffer(
            gameId: game.Id,
            vendor: EVendor.Steam,
            vendorsGameId: vendorsGameId,
            vendorsUrl: game.SteamURL!,
            prices: ExtractPrices(jObj),
            available: true
        );
    }

    private async Task<Game?> GetNewGame(int gameId)
    {
        var (jObj, isGame) = await GetIdContent(gameId);
        if (jObj == null) return null;

        var realId = gameId;
        if (isGame == false)
        {
            var realIdAsString = jObj["data"]?["apps"]?[0]?["id"]?.ToString();
            if (realIdAsString == null) return null;
            
            realId = int.Parse(realIdAsString);
            (jObj, isGame) = await GetIdContent(realId);
        }

        if (jObj == null) return null;

        return ExtractGame(jObj, gameId, realId);
    }

    private Game? ExtractGame(JToken jObj, int gameId, int realId)
    {
        string? name = ExtractName(jObj);
        if (name == null) return null;
        var game = new Game(name: name);
        
        game.Description = ExtractDescription(jObj);
        game.HeaderImage = ExtractThumbnail(jObj);
        game.SteamURL = $"https://store.steampowered.com/app/{realId}";
        game.GameIds.Add(new Game.GameId(EVendor.Steam, gameId.ToString(), realId.ToString()));
        if (gameId != realId) game.InPackages.Add(gameId);
        game.Type = ExtractType(jObj);

        game.Offers.Add(ExtractGameOffer(jObj, game, realId.ToString()));
        return game;
    }

    private async Task<Game?> UpdateGamePrice(Game game)
    {
        var (jObj, isGame) = await GetIdContent(int.Parse(game.GameIds.First(i => i.Vendor == EVendor.Steam).RealId));
        if (jObj == null) return null;
        
        game.Offers.Remove(game.Offers.First(o => o.Vendor == EVendor.Steam));
        game.Offers.Add(ExtractGameOffer(jObj, game, game.GameIds.FirstOrDefault(i => i.Vendor == EVendor.Steam)?.RealId!));
        return game;
    }

    private async Task<(JToken? jObj, bool? isGame)> GetIdContent(int gameId)
    {
        var gameUrl = GameData + gameId + "&l=ru";
        var packageUrl = PackageDetails + gameId + "&l=ru";

        var content = await MakeRequest(gameUrl, gameId);
        var isGame = true;
        if (content == null || content.Contains("\"success\": false"))
        {
            content = await MakeRequest(packageUrl, gameId);
            isGame = false;
        }

        if (content == null) return (null, null);
        return (JsonConvert.DeserializeObject<JObject>(content)?[$"{gameId}"], isGame);
    }
}