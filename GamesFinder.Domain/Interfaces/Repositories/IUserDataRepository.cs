using GamesFinder.Domain.Classes.Entities;

namespace GamesFinder.Domain.Interfaces.Repositories;

public interface IUserDataRepository : IRepository<UserData>
{
    Task<UserData?> GetByUserId(Guid userId);
}