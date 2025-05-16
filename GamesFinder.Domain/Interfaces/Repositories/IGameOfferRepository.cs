using GamesFinder.Domain.Entities;

namespace GamesFinder.Domain.Repositories;

public interface IGameOfferRepository<TEntity> : IRepository<TEntity> where TEntity : GameOffer
{
    Task<ICollection<TEntity>> GetByGameIdAsync(Guid gameId);
    Task<ICollection<TEntity>> GetByVendorIdAsync(Guid vendorId);
}