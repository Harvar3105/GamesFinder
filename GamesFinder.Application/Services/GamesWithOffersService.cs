using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Interfaces.Repositories;
using GamesFinder.Domain.Interfaces.Requests;
using Microsoft.Extensions.Logging;

namespace GamesFinder.Application.Services;

public class GamesWithOffersService : IGameRepository<Game>
{
    private readonly IGameRepository<Game> _gameRepository;
    private readonly IGameOfferRepository<GameOffer> _gameOfferRepository;
    private readonly ILogger<GamesWithOffersService> _logger;

    public GamesWithOffersService(IGameRepository<Game> gameRepository, IGameOfferRepository<GameOffer> gameOfferRepository, ILogger<GamesWithOffersService> logger)
    {
        _gameRepository = gameRepository;
        _gameOfferRepository = gameOfferRepository;
        _logger = logger;
    }
    

    public async Task<bool> SaveAsync(Game entity)
    {
        return await _gameRepository.SaveAsync(entity);
    }

    public async Task<bool> SaveManyAsync(IEnumerable<Game> entities)
    {
        return await _gameRepository.SaveManyAsync(entities);
    }

    public async Task<bool> SaveOrUpdateAsync(Game entity)
    {
        return await _gameRepository.SaveOrUpdateAsync(entity);
    }

    public async Task<bool> SaveOrUpdateManyAsync(IEnumerable<Game> entities)
    {
        return await _gameRepository.SaveOrUpdateManyAsync(entities);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _gameRepository.DeleteAsync(id);
    }

    public async Task<bool> UpdateAsync(Game entity)
    {
        return await _gameRepository.UpdateAsync(entity);
    }
    
    public async Task<List<string>> CheckExistManyBySteamIds(List<string> appIds)
    {
        return await _gameRepository.CheckExistManyBySteamIds(appIds);
    }

    private async Task<ICollection<Game>> FetchManyOffers(ICollection<Game> games)
    {
        var gameIds = games.Select(g => g.Id).ToList();
        var offers = await _gameOfferRepository.GetByGamesIdsAsync(gameIds);

        if (offers == null || games.Count == 0)
        {
            _logger.LogError("Could not fetch games with offers!");
            return games;
        }

        foreach (var game in games)
        {
            var relatedOffers = offers.Where(o => o.GameId.Equals(game.Id)).ToList();
            if (relatedOffers.Count == 0) continue;

            game.Offers = relatedOffers;
        }

        return games;
    }

    private async Task<Game?> FetchOneOffer(Game? game)
    {
        if (game == null) return null;
        
        game.Offers = (await _gameOfferRepository.GetByGameIdAsync(game.Id))?.ToList() ?? [];
        return game;
    }

    public async Task<ICollection<Game>?> GetAllAsync()
    {
        var games = await _gameRepository.GetAllAsync();
        if (games == null) return null;
        return await FetchManyOffers(games);
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        return await FetchOneOffer(await _gameRepository.GetByIdAsync(id));
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _gameRepository.ExistsAsync(id);
    }

    public async Task<long> CountAsync()
    {
        return await _gameRepository.CountAsync();
    }

    public async Task<ICollection<Game>> GetPagedAsync(int page, int pageSize)
    {
        var games = await _gameRepository.GetPagedAsync(page, pageSize);
        return await FetchManyOffers(games);
    }

    public async Task<ICollection<Game>?> GetPagedWithFiltersAsync(int page, int pageSize, GamesFilters filters)
    {
        var games = await _gameRepository.GetPagedWithFiltersAsync(page, pageSize, filters);
        if (games is null) return null;
        return await FetchManyOffers(games);
    }

    public async Task<Game?> GetByAppId(int appId)
    {
        return await FetchOneOffer(await _gameRepository.GetByAppId(appId));
    }

    public async Task<List<Game>?> GetByAppIds(IEnumerable<int> appIds)
    {
        var games = await _gameRepository.GetByAppIds(appIds);
        if (games == null) return null;
        return (await FetchManyOffers(games)).ToList();
    }

    public async Task<Game?> GetByAppNameAsync(string appName)
    {
        return await FetchOneOffer(await _gameRepository.GetByAppNameAsync(appName));
    }

    public async Task<bool> ExistsByAppIdAsync(int appId)
    {
        return await _gameRepository.ExistsByAppIdAsync(appId);
    }

    public async Task<bool> ExistsByAppNameAsync(string appName)
    {
        return await _gameRepository.ExistsByAppNameAsync(appName);
    }
}