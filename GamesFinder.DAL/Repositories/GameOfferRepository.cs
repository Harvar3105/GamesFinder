using GamesFinder.Domain.Entities;
using GamesFinder.Domain.Repositories;
using MongoDB.Driver;

namespace GamesFinder.DAL.Repositories;

public class GameOfferRepository : Repository<GameOffer>, IGameOfferRepository<GameOffer>
{
    public GameOfferRepository(IMongoDatabase database) : base(database, "gameoffer")
    {
    }

    public async Task<ICollection<GameOffer>> GetByGameIdAsync(Guid gameId)
    {
        return await Collection.Find(e => e.GameId == gameId).ToListAsync();
    }

    public async Task<ICollection<GameOffer>> GetByVendorIdAsync(Guid vendorId)
    {
        return await Collection.Find(e => e.VendorId == vendorId).ToListAsync();
    }
}