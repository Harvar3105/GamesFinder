using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Interfaces.Crawlers;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GamesFinder.Application.Crawlers;

public abstract class Crawler : ICrawler
{
    protected static readonly HttpClient Client = new();
    protected string GameData;
    protected readonly IGameOfferRepository<GameOffer> GameOfferRepository;
    protected readonly IGameRepository<Game> GameRepository;
    protected readonly ILogger<Crawler> Logger;

    protected Crawler(string gameData, IGameOfferRepository<GameOffer> gameOfferRepository, IGameRepository<Game> gameRepository, ILogger<Crawler> logger)
    {
        GameData = gameData;
        GameOfferRepository = gameOfferRepository;
        GameRepository = gameRepository;
        Logger = logger;
        
        Client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (compatible; EducationalBot/1.0)");
    }

    public abstract Task CrawlGamesAsync(ICollection<int>? gameIds, bool force = false);
    public abstract Task CrawlPricesAsync(ICollection<Game>? games, bool force = false);
    protected abstract Task<(Game?, GameOffer?, bool)> ExtractData(string content, string url, int? appId = null, Game? existingGame = null);
    protected async Task SaveOrUpdateBulk(List<Game> games)
    {
        if (games.Count == 0) return;
        
        List<GameOffer> gamesOffers = new();
        foreach (var game in games)
        {
            gamesOffers.AddRange(game.Offers);
        }

        await SaveOrUpdateBulkGames(games);
        await SaveOrUpdateBulkGameOffers(gamesOffers);
    }

    protected async Task SaveOrUpdateBulkGames(List<Game> games)
    {
        if (games.Count == 0) return;
        
        var success1 = await GameRepository.SaveManyAsync(games);
        if (!success1)
        {
            Logger.LogError("Something went wrong, couldn't save games");
        }
        games.Clear();
    }

    protected async Task SaveOrUpdateBulkGameOffers(List<GameOffer> gamesOffers)
    {
        if (gamesOffers.Count == 0) return;
        
        var success2 = await GameOfferRepository.SaveManyAsync(gamesOffers);
        if (!success2)
        {
            Logger.LogError("Something went wrong, couldn't save games");
        }
        
        gamesOffers.Clear();
    }
}