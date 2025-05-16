using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Entities;

namespace GamesFinder.Domain.Repositories;

public interface IGameRepository<TEntity> : IRepository<TEntity> where TEntity : Game
{
    Task<Game> GetByAppId(int appId);
    Task<List<Game>> GetByAppIds(IEnumerable<int> appIds);
}