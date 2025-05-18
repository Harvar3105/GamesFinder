using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace GamesFinder.DAL.Repositories;

public class UserDataRepository: Repository<UserData>, IUserDataRepository
{
    public UserDataRepository(IMongoDatabase database, string collectionName, ILogger<Repository<UserData>> logger) : base(database, collectionName, logger)
    {
    }

    public async Task<UserData?> GetByUserId(Guid userId)
    {
        return await Collection.Find(e => e.UserId.Equals(userId)).FirstAsync();
    }
}