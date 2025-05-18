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
            .Find(g => g.GameIds.Any(v => v.Equals(appId.ToString()))).AnyAsync();
    }
}