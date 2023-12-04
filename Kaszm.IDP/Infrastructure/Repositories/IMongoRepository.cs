using System.Linq.Expressions;
using System.Reflection;
using IdentityServer.Attributes;
using IdentityServer.Models;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace IdentityServer.Infrastructure.Repositories;

public interface IMongoRepository<TModel> : IDatabaseRepository<TModel> where TModel : BaseEntity
{
    public static string GetConcurrencyTimestamp()
    {
        return Guid.NewGuid().ToString();
    }
    
    // as this is mongo specific repo, we should name it GetRecords?
    Task<IEnumerable<TReturn>> GetRecordsAsync<TReturn>(Expression<Func<TModel, bool>> where,
        Expression<Func<TModel, TReturn>> projection);
}

public class MongoRepository<TModel> : IMongoRepository<TModel> where TModel : BaseEntity
{
    private readonly IMongoCollection<TModel> _mongoCollection;

    public MongoRepository(IMongoClient mongoClient, IDatabaseSettings settings)
    {
        ArgumentNullException.ThrowIfNull(mongoClient);
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        var collectionAttribute = typeof(TModel).GetCustomAttribute<MongoCollectionAttribute>();
        var collectionName = collectionAttribute?.Name;
        if (string.IsNullOrWhiteSpace(collectionName))
        {
            throw new InvalidOperationException("collection name cannot be empty");
        }

        _mongoCollection = database.GetCollection<TModel>(collectionName);
    }

    public async Task CreateAsync(TModel data)
    {
        ArgumentNullException.ThrowIfNull(data);

        // generate concurrency timestamp on each operation
        data.ConcurrencyTimestamp = IMongoRepository<TModel>.GetConcurrencyTimestamp();
        data.CreatedAt = DateTime.UtcNow;
        data.IsActive = true;
        
        await _mongoCollection.InsertOneAsync(data);
    }

    public async Task UpdateAsync(TModel data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var filterDefinition = Builders<TModel>.Filter.Where(x => x.Id == data.Id);
        
        // generate concurrency timestamp on each operation
        data.ConcurrencyTimestamp = IMongoRepository<TModel>.GetConcurrencyTimestamp();
        data.LastUpdatedAt = DateTime.UtcNow;

        await _mongoCollection.ReplaceOneAsync(filterDefinition, data);
    }

    public async Task<TModel> GetAsync(Expression<Func<TModel, bool>> where)
    {
        var collectionAsQueryable = _mongoCollection.AsQueryable();
        if (where is not null)
        {
            collectionAsQueryable = collectionAsQueryable.Where(where);
        }

        return await collectionAsQueryable.FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<TReturn>> GetRecordsAsync<TReturn>(Expression<Func<TModel, bool>> where,
        Expression<Func<TModel, TReturn>> projection)
    {
        ArgumentNullException.ThrowIfNull(projection);
        var collectionAsQueryable = _mongoCollection.AsQueryable();
        if (where is not null)
        {
            collectionAsQueryable = collectionAsQueryable.Where(where);
        }

        return await collectionAsQueryable.Select(projection).ToListAsync();
    }
}