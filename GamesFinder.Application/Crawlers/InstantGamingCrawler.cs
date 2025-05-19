using System.Text.RegularExpressions;
using AngleSharp;
using GamesFinder.Application.Crawlers;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Crawlers;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GamesFinder.Application;

public class InstantGamingCrawler :  Crawler, ICrawler
{
    private static readonly string Pattern = @"^(.*?)(?:\s*\([^)]+\))?(?:\s*-\s*.+)?$";
    private static readonly int MaxCalls = 25_000;
    private static readonly int SaveInterval = 200;
    private static IBrowsingContext  _browsingContext;
    private readonly GameSteamAppIdFinder _appIdFinder;

    public InstantGamingCrawler(
        IGameOfferRepository<GameOffer> gameOfferRepository,
        IGameRepository<Game> gameRepository,
        ILogger<InstantGamingCrawler> logger,
        GameSteamAppIdFinder appIdFinder
        ) : 
        base(
            "https://www.instant-gaming.com/",
            gameOfferRepository,
            gameRepository,
            logger)
    {
        this._appIdFinder = appIdFinder;
        _browsingContext = BrowsingContext.New(Configuration.Default);
    }

    public override async Task CrawlGamesAsync(ICollection<int>? gameIds, bool force = false)
    {
        if (gameIds == null)
        {
            await CrawlAllGames();
            return;
        }
        
        var offers = new List<GameOffer>();
        var callsCount = 10;

        var games = await GameRepository.GetByAppIds(gameIds);
        foreach (Game game in games)
        {
            if (callsCount != 0 && callsCount % SaveInterval == 0)
            {
                Logger.LogInformation("Saving offers.");
                await SaveOrUpdateBulkGameOffers(offers);
            }

            var vendorsId = game.GameIds.First(i => i.Vendor == EVendor.InstantGaming)?.Id;
            if (vendorsId == null)
            {
                Logger.LogError($"No Vendor id found for game {game.Name}!");
                continue;
            }

            var constructedUrl = GameData + vendorsId + '-';
            var response = await Client.GetAsync(new Uri(constructedUrl));
            callsCount++;
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError($"Error getting game data for {vendorsId}, ${response.StatusCode}\n URL: {constructedUrl}");
                continue;
            }
            
            var page = await response.Content.ReadAsStringAsync();
            var (relatedGame, newOffer, isNewGame) = await ExtractData(content: page, url: constructedUrl, existingGame: game);
            if (newOffer == null) continue;
            
            offers.Add(newOffer);
        }
        await SaveOrUpdateBulkGameOffers(offers);
    }

    private async Task CrawlAllGames()
    {
        var games = new List<Game>(); //new offers and games
        var offers = new List<GameOffer>(); //Only new offers
        var callsCount = 10;

        while (callsCount < MaxCalls)
        {
            if (callsCount % SaveInterval == 0)
            {
                Logger.LogInformation("Saving games and offers.");
                await SaveOrUpdateBulk(games);
                await SaveOrUpdateBulkGameOffers(offers);
            }

            var constructedUrl = GameData + callsCount + '-';
            
            var response = await Client.GetAsync(new Uri(constructedUrl));
            callsCount++;
            if (!response.IsSuccessStatusCode)
            {
                Logger.LogError($"Error getting game data for {callsCount}, ${response.StatusCode}\n URL: {constructedUrl}");
                continue;
            }
            
            var page = await response.Content.ReadAsStringAsync();
            
            var (game, offer, isNewGame) = await ExtractData(content:page, url: constructedUrl, appId: callsCount);
            if (isNewGame)
            {
                games.Add(game!);
            }
            else
            {
                if (offer == null) continue;
            }
            
            offers.Add(offer!);
        }

        await SaveOrUpdateBulk(games);
        await SaveOrUpdateBulkGameOffers(offers);
    }

    public override Task CrawlPricesAsync(ICollection<Game>? games, bool force = false)
    {
        throw new NotImplementedException();
    }

    protected override async Task<(Game?, GameOffer?, bool)> ExtractData(string content, string url, int? appId = null, Game? existingGame = null)
    {
        bool newGame = false;
        var doc = await _browsingContext.OpenAsync(req => req.Content(content));

        var gameNameElement = doc.QuerySelector("h1.game-title");
        if (gameNameElement == null) return (null, null, false);
        var gameName = gameNameElement.TextContent.Trim();
        if (string.IsNullOrEmpty(gameName))
        {
            Logger.LogWarning("Could not find game name!");
            return (null, null, false);
        }
        
        var regexMatch = Regex.Match(gameName, Pattern);
        if (!regexMatch.Success)
        {
            Logger.LogWarning("Could not normalize game name!");
            return (null, null, false);
        }
        var normalizedName = regexMatch.Groups[1].Value.Trim();

        // var editionElement = doc.QuerySelector("select.other-products-choices option[selected]");
        // if (editionElement != null)
        // {
        //      = editionElement.TextContent.Trim();
        // }
        
        var game = existingGame ?? await GameRepository.GetByAppNameAsync(normalizedName); //provided or search or create if possible
        if (game == null)
        {
            game = _appIdFinder.FindAppId(normalizedName);
            if (game == null)
            {
                Logger.LogWarning($"Could not find or create game {normalizedName}!");
                return (null, null, false);
            }
            newGame = true;
        };
        
        if (game.GameIds.All(i => i.Vendor != EVendor.InstantGaming)) game.GameIds.Add(new Game.GameId(EVendor.InstantGaming, appId.ToString()!));
        
        var gamePriceElement = doc.QuerySelector("div.total");
        if (gamePriceElement == null)
        {
            Logger.LogWarning("Could not find game price!");
            return (null, null, false);
        }
        var amount = gamePriceElement.TextContent.Trim();
        var normalizedAmount = amount.Replace("$", "").Replace("€", "");
        
        var availabilityElement = doc.QuerySelector("div.nostock");
        var availability = availabilityElement == null;
        
        Dictionary<ECurrency, GameOffer.PriceRange> prices = new();
        switch (amount.Last())
        {
            case '€':
                prices.Add(ECurrency.Eur, new GameOffer.PriceRange(null, decimal.Parse(normalizedAmount)));
                break;
            case '$':
                prices.Add(ECurrency.Usd, new GameOffer.PriceRange(null, decimal.Parse(normalizedAmount)));
                break;
            case '_':
                prices.Add(ECurrency.Eur, new GameOffer.PriceRange(null, null));
                break;
        }

        var offer = new GameOffer(
            gameId: game.Id,
            vendor: EVendor.InstantGaming,
            vendorsUrl: url,
            available: availability,
            prices: prices
        );

        game.Offers.Add(offer);
        return (game, offer, newGame);
    }
}