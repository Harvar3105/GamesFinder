using GamesFinder.Domain.Classes.Entities;

namespace GamesFinder.Domain.Interfaces.Repositories;

public interface IGameRepository<TEntity> : IRepository<TEntity> where TEntity : Game
{
    Task<Game> GetByAppId(int appId);
    Task<List<Game>> GetByAppIds(IEnumerable<int> appIds);
}