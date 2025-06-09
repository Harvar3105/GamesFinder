using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GamesFinder.DAL.Repositories;

public class UnprocessedGamesRepository : Repository<UnprocessedGame>, IUnprocessedGamesRepository<UnprocessedGame>
{
    private readonly ILogger<UnprocessedGamesRepository> _logger;
    public UnprocessedGamesRepository(IMongoDatabase database, ILogger<UnprocessedGamesRepository> logger) : base(database, "unprocessed_games", logger)
    {
        _logger = logger;
    }

    public async Task<UnprocessedGame?> GetBySteamIdAsync(int steamId)
    {
        return await Collection.Find(e => e.SteamId == steamId).FirstOrDefaultAsync();
    }

    public async Task<UnprocessedGame?> GetBySteamNameAsync(string steamName)
    {
        return await Collection.Find(e => e.SteamName.Contains(steamName)).FirstOrDefaultAsync();
    }

    public async Task<UnprocessedGame?> GetByVendorIdAsync(EVendor vendor, string vendorId)
    {
        return await Collection.Find(e => e.VendorsId!.Equals(vendorId)).FirstOrDefaultAsync();
    }

    public async Task<UnprocessedGame?> GetByVendorNameAsync(EVendor vendor, string vendorName)
    {
        return await Collection.Find(e => e.VendorsName.Contains(vendorName)).FirstOrDefaultAsync();
    }
}