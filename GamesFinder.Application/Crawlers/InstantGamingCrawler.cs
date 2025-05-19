using System.Text.RegularExpressions;
using AngleSharp;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Crawlers;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GamesFinder.Application;

public class InstantGamingCrawler :  Crawler, ICrawler
{
    private static readonly string pattern = @"^(.*?)\\s*(\\([^)]+\\))?\\s*(-\\s*.+)?$\n";
    private static readonly int MaxCalls = 25_000;
    private static readonly int SaveInterval = 200;
    private static IBrowsingContext  browsingContext;
    private readonly GameSteamAppIdFiner appIdFinder;

    public InstantGamingCrawler(
        IGameOfferRepository<GameOffer> gameOfferRepository,
        IGameRepository<Game> gameRepository,
        ILogger<InstantGamingCrawler> logger,
        GameSteamAppIdFiner appIdFinder
        ) : 
        base(
            "https://www.instant-gaming.com/",
            gameOfferRepository,
            gameRepository,
            logger)
    {
        this.appIdFinder = appIdFinder;
        browsingContext = BrowsingContext.New(Configuration.Default);
    }

    public override async Task CrawlGamesAsync(ICollection<int> gameIds, bool force = false)
    {
        var games = new List<Game>(); //new offers and games
        var offers = new List<GameOffer>(); //Only new offers
        var callsCount = 0;

        while (callsCount < MaxCalls)
        {
            if (callsCount != 0 && callsCount % SaveInterval == 0)
            {
                await SaveOrUpdateBulk(games);
                await SaveOrUpdateBulkGameOffers(offers);
            }

            if (!force)
            {
                if (await _gameRepository.ExistsByAppIdAsync(callsCount)) continue;
            }

            var constructedUrl = GameData + callsCount + '-';
            
            var response = await Client.GetAsync(new Uri(constructedUrl));
            callsCount++;

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"Error getting game data for {callsCount}, ${response.StatusCode}");
                continue;
            }
            
            var page = await response.Content.ReadAsStringAsync();
            
            var (game, offer, isNewGame) = await ExtractData(callsCount, page, constructedUrl);
            if (isNewGame)
            {
                games.Add(game!);
            }
            else
            {
                if (offer == null) continue;
            }
            
            offers.Add(offer);
        }

        await SaveOrUpdateBulk(games);
        await SaveOrUpdateBulkGameOffers(offers);
    }

    public override Task CrawlPricesAsync(ICollection<Game> games, bool force = false)
    {
        throw new NotImplementedException();
    }

    protected override async Task<(Game?, GameOffer?, bool)> ExtractData(int appId, string content, string url)
    {
        bool newGame = false;
        var doc = await browsingContext.OpenAsync(req => req.Content(content));

        var gameNameElement = doc.QuerySelector("h1.game-title");
        if (gameNameElement == null) return (null, null, false);
        var gameName = gameNameElement.TextContent.Trim();
        if (string.IsNullOrEmpty(gameName)) return (null, null, false);
        
        var regexMatch = Regex.Match(gameName, pattern);
        if (!regexMatch.Success) return (null, null, false);
        var normalizedName = regexMatch.Groups[1].Value.Trim();
        _logger.LogInformation($"Normalized name for {gameName}  to {normalizedName}");
        
        var game = await _gameRepository.GetByAppNameAsync(normalizedName);
        if (game == null)
        {
            game = appIdFinder.FindAppId(normalizedName);
            if (game == null) return (null, null, false);
            newGame = true;
        };
        
        var gamePriceElement = doc.QuerySelector("div.total");
        if (gamePriceElement == null) return (null, null, false);
        var ammount = gamePriceElement.TextContent.Trim();
        var cleanAmmount = ammount.Substring(0, ammount.Length - 1);
        
        var availabilityElement = doc.QuerySelector("div.nostock");
        var availability = availabilityElement == null;
        
        Dictionary<ECurrency, GameOffer.PriceRange> prices = new();
        switch (ammount[-1])
        {
            case '€':
                prices.Add(ECurrency.Eur, new GameOffer.PriceRange(null, decimal.Parse(cleanAmmount)));
                break;
            case '$':
                prices.Add(ECurrency.Usd, new GameOffer.PriceRange(null, decimal.Parse(cleanAmmount)));
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