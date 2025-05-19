using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GamesFinder.DAL.Repositories;

public class GameRepository : Repository<Game>, IGameRepository<Game>
{
    private IGameOfferRepository<GameOffer> _gameOfferRepository;
    public GameRepository(IMongoDatabase database, ILogger<GameRepository> logger, IGameOfferRepository<GameOffer> gameOfferRepository) : base(database, "game", logger)
    {
        _gameOfferRepository = gameOfferRepository;
    }

    public async Task<Game?> GetByAppId(int appId)
    {
        try
        {
            var result = await Collection
                                     .Find(g => g.GameIds.Any(v => v.Id.Equals(appId.ToString())))
                                     .FirstOrDefaultAsync();
            if (result != null)
            {
                result.Offers = (await _gameOfferRepository.GetByGameIdAsync(result.Id))?.ToList() ?? new List<GameOffer>();
            }

            return result;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
            return null;
        }
    }

    public async Task<List<Game>?> GetByAppIds(IEnumerable<int> appIds)
    {
        try
        {
            var result = await Collection
                .Find(g => g.GameIds.Any(v => appIds.Contains(int.Parse(v.Id)))).ToListAsync();

            foreach (var game in result)
            {
                game.Offers = (await _gameOfferRepository.GetByGameIdAsync(game.Id))?.ToList() ?? new List<GameOffer>();
            }
            
            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
            return null;
        }
    }

    public async Task<bool> ExistsByAppIdAsync(int appId)
    {
        try
        {
            return await Collection
                .Find(g => g.GameIds.Any(v => v.Id.Equals(appId.ToString()))).AnyAsync();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex.Message);
            return false;
        }
        
    }

    public async Task<bool> ExistsByAppNameAsync(string appName)
    {
        try
        {
            var allProducts = (await Collection
                .Find(_ => true)
                .Project(p => p.Name)
                .ToListAsync()).OrderBy(g => g.Length);
                    
            
            return allProducts.Any(name =>
                appName.Contains(name, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
            return false;
        }
        
    }

    public async Task<Game?> GetByAppNameAsync(string appName)
    {
        try
        {
            var allProducts = (await Collection
                .Find(_ => true)
                .ToListAsync())
                .OrderBy(g => g.Name.Length);

            var result = allProducts.First(g => appName.Contains(g.Name, StringComparison.OrdinalIgnoreCase));

            if (result != null)
            {
                result.Offers = (await _gameOfferRepository.GetByGameIdAsync(result.Id))?.ToList() ?? new List<GameOffer>();
            }
            
            return result;
        }
        catch (Exception e)
        {
            Logger.LogError(e.Message);
            return null;
        }
        
    }
}