using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GamesFinder.DAL.Repositories;

public class GameRepository : Repository<Game>, IGameRepository<Game>
{
    public GameRepository(IMongoDatabase database, ILogger<GameRepository> logger) : base(database, "game", logger)
    {
        
    }

    public async Task<Game> GetByAppId(int appId)
    {
        return await Collection
            .Find(g => g.GameIds.Any(v => v.Id.Equals(appId.ToString())))
            .FirstOrDefaultAsync();
    }

    public async Task<List<Game>> GetByAppIds(IEnumerable<int> appIds)
    {
        return await Collection
            .Find(g => g.GameIds.Any(v => appIds.Contains(int.Parse(v.Id)))).ToListAsync();
    }

    public async Task<bool> ExistsByAppIdAsync(int appId)
    {
        return await Collection
            .Find(g => g.GameIds.Any(v => v.Id.Equals(appId.ToString()))).AnyAsync();
    }

    public async Task<bool> ExistsByAppNameAsync(string appName)
    {
        var allProducts = (await Collection
            .Find(_ => true)
            .Project(p => p.Name)
            .ToListAsync()).OrderBy(g => g.Length);
        

        return allProducts.Any(name =>
            appName.Contains(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Game?> GetByAppNameAsync(string appName)
    {
        var allProducts = (await Collection
            .Find(_ => true)
            .ToListAsync())
            .OrderBy(g => g.Name.Length);

        return allProducts.First(g => appName.Contains(g.Name, StringComparison.OrdinalIgnoreCase));
    }
}