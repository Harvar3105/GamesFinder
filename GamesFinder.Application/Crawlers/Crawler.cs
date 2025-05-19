using AngleSharp.Html.Parser;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Crawlers;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace GamesFinder.Application;

public abstract class Crawler : ICrawler
{
    protected static readonly HttpClient Client = new();
    protected string GameData;
    protected readonly IGameOfferRepository<GameOffer> _gameOfferRepository;
    protected readonly IGameRepository<Game> _gameRepository;
    protected readonly ILogger<Crawler> _logger;

    protected Crawler(string gameData, IGameOfferRepository<GameOffer> gameOfferRepository, IGameRepository<Game> gameRepository, ILogger<Crawler> logger)
    {
        GameData = gameData;
        _gameOfferRepository = gameOfferRepository;
        _gameRepository = gameRepository;
        _logger = logger;
    }

    public abstract Task CrawlGamesAsync(ICollection<int> gameIds, bool force = false);
    public abstract Task CrawlPricesAsync(ICollection<Game> games, bool force = false);
    protected abstract Task<(Game?, GameOffer?, bool)> ExtractData(int appId, string content, string url);
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
        
        var success1 = await _gameRepository.SaveManyAsync(games);
        if (!success1)
        {
            _logger.LogError("Something went wrong, couldn't save games");
        }
        games.Clear();
    }

    protected async Task SaveOrUpdateBulkGameOffers(List<GameOffer> gamesOffers)
    {
        if (gamesOffers.Count == 0) return;
        
        var success2 = await _gameOfferRepository.SaveManyAsync(gamesOffers);
        if (!success2)
        {
            _logger.LogError("Something went wrong, couldn't save games");
        }
        
        gamesOffers.Clear();
    }
}