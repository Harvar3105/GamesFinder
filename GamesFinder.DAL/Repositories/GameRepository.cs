using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Entities;
using GamesFinder.Domain.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GamesFinder.DAL;

public class GameRepository : Repository<Game>, IGameRepository<Game>
{
    public GameRepository(IMongoDatabase database, ILogger<GameRepository> logger) : base(database, "game", logger)
    {
        
    }
}