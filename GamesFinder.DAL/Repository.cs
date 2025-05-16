using GamesFinder.Domain.Entities;
using GamesFinder.Domain.Repositories;
using MongoDB.Driver;

namespace GamesFinder.DAL;

public class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly IMongoCollection<T> Collection;

    public Repository(IMongoDatabase database, string collectionName)
    {
        Collection = database.GetCollection<T>(collectionName);
    }
    
    public async Task<bool> SaveAsync(T entity)
    {
        await Collection.InsertOneAsync(entity);
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await Collection.DeleteOneAsync(e => e.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        var result = await Collection.ReplaceOneAsync(e => e.Id == entity.Id, entity);
        return result.ModifiedCount > 0;
    }

    public async Task<ICollection<T>> GetAllAsync()
    {
        return await Collection.Find(_ => true).ToListAsync();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await Collection.Find(e => e.Id == id).FirstOrDefaultAsync();
    }
}