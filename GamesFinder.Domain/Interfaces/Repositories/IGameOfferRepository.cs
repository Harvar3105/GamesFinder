using GamesFinder.Domain.Classes.Entities;

namespace GamesFinder.Domain.Interfaces.Repositories;

public interface IGameOfferRepository<TEntity> : IRepository<TEntity> where TEntity : GameOffer
{
    Task<ICollection<TEntity>> GetByGameIdAsync(Guid gameId);
    Task<ICollection<TEntity>> GetByVendorAsync(string vendor);
}