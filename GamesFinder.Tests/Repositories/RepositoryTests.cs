using GamesFinder.DAL.Repositories;
using GamesFinder.Domain.Classes.Entities;
using GamesFinder.Domain.Enums;
using GamesFinder.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Mongo2Go;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace GamesFinder.Tests.Repositories;

public abstract class RepositoryTests<TRepo, TEntity> : IDisposable
    where TRepo : IRepository<TEntity>
    where TEntity : Entity
{
    static RepositoryTests()
    {
        BsonSerializer.RegisterSerializer(new EnumSerializer<ECurrency>(BsonType.String));
    }
    
    protected readonly MongoDbRunner _mongoRunner;
    protected readonly IMongoDatabase _database;
    protected readonly TRepo _repository;
    
    public RepositoryTests(string dbName, Func<IMongoDatabase, ILogger<TRepo>, TRepo> repositoryFactory)
    {
        _mongoRunner = MongoDbRunner.Start();
        var client = new MongoClient(_mongoRunner.ConnectionString);
        _database = client.GetDatabase(dbName);

        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger<TRepo>();

        _repository = repositoryFactory(_database, logger);
    }
    
    public void Dispose()
    {
        _mongoRunner.Dispose();
    }
}