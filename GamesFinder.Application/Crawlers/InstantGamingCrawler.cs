using System.Text.RegularExpressions;
using AngleSharp;
using AngleSharp.Dom;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Crawlers;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GamesFinder.Application.Crawlers;

public class InstantGamingCrawler :  Crawler, ICrawler
{
    private static readonly string Pattern = @"^(.*?)(?:\s*\([^)]+\))?(?:\s*-\s*.+)?$";
    // private static readonly int MaxCalls = 25_000;
    private static readonly int SaveInterval = 200;
    private static readonly int ForbiddenStart = 10; // 10 first ids are not games
    private static readonly int CooldownMinutes = 5;
    private static IBrowsingContext _browsingContext;
    private readonly GameSteamAppIdFinder _appIdFinder;
    private readonly IUnprocessedGamesRepository<UnprocessedGame> _unprocessedGamesRepository;

    public InstantGamingCrawler(
        IGameOfferRepository<GameOffer> gameOfferRepository,
        IGameRepository<Game> gameRepository,
        ILogger<InstantGamingCrawler> logger,
        GameSteamAppIdFinder appIdFinder,
        IUnprocessedGamesRepository<UnprocessedGame> unprocessedGamesRepository
        ) : base(
            "https://www.instant-gaming.com/",
            gameOfferRepository,
            gameRepository,
            logger)
    {
        _appIdFinder = appIdFinder;
        _unprocessedGamesRepository = unprocessedGamesRepository;
        _browsingContext = BrowsingContext.New(Configuration.Default);
    }


    public override async Task CrawlGamesAsync(ICollection<int>? gameIds, bool force = false)
    {
        if (gameIds == null)
        {
            Logger.LogCritical("No gameIds were provided.");
            return;
        }
        
        var games = await GameRepository.GetByAppIds(gameIds);
        if (games == null)
        {
            Logger.LogCritical("Such games doesn't exist.");
            return;
        }
        
        foreach (Game game in games)
        {
            var vendorsId = game.GameIds.FirstOrDefault(i => i.Vendor == EVendor.InstantGaming)?.RealId;
            if (vendorsId == null)
            {
                Logger.LogCritical($"{game.Name} doesn't have InstantGaming ID.");
                continue;
            }
            
            var content = await GetContent(int.Parse(vendorsId));
            game.Offers.Add(ExtractGameOffer(content, game.Id, int.Parse(vendorsId)));
        }
        
        var offers = new List<GameOffer>();
        games.ForEach(g => offers.AddRange(g.Offers));
        await SaveOrUpdateBulkGameOffers(offers);
    }

    public async Task CrawlAllGamesAsync(int maxCalls, bool force = false)
    {
        var games = new List<Game>();
        var unprocessedGames = new List<UnprocessedGame>();
        var callsCount = ForbiddenStart;

        while (callsCount < maxCalls)
        {
            if (callsCount % SaveInterval == 0)
            {
                var offers = new List<GameOffer>();
                games.ForEach(g => offers.AddRange(g.Offers));
                await SaveOrUpdateBulkGameOffers(offers);
                await _unprocessedGamesRepository.SaveOrUpdateManyAsync(unprocessedGames);
                unprocessedGames.Clear();
            }

            var vendorsId = callsCount; //Easier to understand

            var content = await GetContent(vendorsId);
            callsCount++;
            var unprocessedGame = FindGame(content);
            if (unprocessedGame == null) continue;

            var game = await GameRepository.GetByAppId(unprocessedGame.SteamId);
            if (game == null)
            {
                unprocessedGames.Add(unprocessedGame);
            }
            else
            {
                game.Offers.Add(ExtractGameOffer(content, game.Id, vendorsId));
                games.Add(game);
            }
        }
    }

    private bool ExtractAvailability(IDocument doc)
    {
        var availabilityElement = doc.QuerySelector("div.nostock");
        return availabilityElement == null;
    }

    private GameOffer ExtractGameOffer(IDocument doc, Guid gameId, int vendorsId)
    {
        return new GameOffer(
            gameId: gameId,
            vendor: EVendor.InstantGaming,
            vendorsGameId: vendorsId.ToString(),
            vendorsUrl: GameData + vendorsId + '-',
            available: ExtractAvailability(doc),
            prices: ExtractPrice(doc)
        );
    }

    private Dictionary<ECurrency, GameOffer.PriceRange> ExtractPrice(IDocument doc)
    {
        Dictionary<ECurrency, GameOffer.PriceRange> prices = new();
        
        var gamePriceElement = doc.QuerySelector("div.total");
        if (gamePriceElement == null)
        {
            prices.Add(ECurrency.Eur, new GameOffer.PriceRange(null, null));
            return prices;
        }
        
        var amount = gamePriceElement.TextContent.Trim();
        var normalizedAmount = amount.Replace("$", "").Replace("€", "");
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
        
        return prices;
    }

    private UnprocessedGame? FindGame(IDocument doc)
    {
        var gameName = doc.QuerySelector("h1.game-title")?.TextContent.Trim();
        if (string.IsNullOrEmpty(gameName))
        {
            Logger.LogError("Could not extract game name!");
            return null;
        }
        
        var normalizedName = NormalizeGameName(gameName);

        var unprocessedGame = _appIdFinder.FindApp(normalizedName);
        if (unprocessedGame == null)
        {
            Logger.LogError($"Could not find game with name {gameName}\nNormalized name: {normalizedName}");
        }

        return unprocessedGame;
    }

    private string NormalizeGameName(string gameName)
    {
        var regexMatch = Regex.Match(gameName, Pattern);
        if (!regexMatch.Success)
        {
            Logger.LogWarning("Could not normalize game name!");
            return gameName;
        }
        return regexMatch.Groups[1].Value.Trim();
    }

    private async Task<IDocument> GetContent(int gameId)
    {
        var constructedUrl = GameData + gameId + '-';
        var content = await MakeRequest(constructedUrl, gameId);

        return await _browsingContext.OpenAsync(req => req.Content(content));
    }
}