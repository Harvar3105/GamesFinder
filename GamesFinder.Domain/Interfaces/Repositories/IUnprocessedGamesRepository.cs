using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;

namespace GamesFinder.Domain.Interfaces.Repositories;

public interface IUnprocessedGamesRepository<TEntity> : IRepository<TEntity> where TEntity : Entity
{
    Task<UnprocessedGame?> GetBySteamIdAsync(int steamId);
    Task<UnprocessedGame?> GetBySteamNameAsync(string steamName);
    Task<UnprocessedGame?> GetByVendorIdAsync(EVendor vendor, string vendorId);
    Task<UnprocessedGame?> GetByVendorNameAsync(EVendor vendor, string vendorName);
}