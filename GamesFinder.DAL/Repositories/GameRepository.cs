using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Repositories;
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
        return await Collection.Find(g => g.AppId == appId).FirstOrDefaultAsync();
    }

    public async Task<List<Game>> GetByAppIds(IEnumerable<int> appIds)
    {
        return await Collection.Find(g => appIds.Contains(g.AppId ?? 0)).ToListAsync();
    }
}