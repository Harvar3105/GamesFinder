using GamesFinder.Domain.Entities;
using GamesFinder.Domain.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GamesFinder.DAL.Repositories;

public class GameOfferRepository : Repository<GameOffer>, IGameOfferRepository<GameOffer>
{
    public GameOfferRepository(IMongoDatabase database, ILogger<GameOfferRepository> logger) : base(database, "gameoffer", logger)
    {
    }

    public async Task<ICollection<GameOffer>> GetByGameIdAsync(Guid gameId)
    {
        return await Collection.Find(e => e.GameId == gameId).ToListAsync();
    }

    public async Task<ICollection<GameOffer>> GetByVendorAsync(string vendor)
    {
        return await Collection.Find(e => e.Vendor == vendor).ToListAsync();
    }
}