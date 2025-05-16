using GamesFinder.Domain.Entities;
using GamesFinder.Domain.Repositories;
using MongoDB.Driver;

namespace GamesFinder.DAL;

public class GameRepository : Repository<Game>, IGameRepository<Game>
{
    public GameRepository(IMongoDatabase database) : base(database, "game")
    {
        
    }
}