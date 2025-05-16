using GamesFinder.Domain.Entities;

namespace GamesFinder.Domain.Repositories;

public interface IRepository<TEntity> where TEntity : Entity
{
    Task<bool> SaveAsync(TEntity entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> UpdateAsync(TEntity entity);
    Task<ICollection<TEntity>> GetAllAsync();
    Task<TEntity?> GetByIdAsync(Guid id);
}