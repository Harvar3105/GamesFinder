using GamesFinder.Domain.Entities;

namespace GamesFinder.Domain.Repositories;

public interface IGameRepository<TEntity> : IRepository<TEntity> where TEntity : Game
{
    
}