using IdentityServer.Models;
using MongoDB.Driver;

namespace IdentityServer.Infrastructure.Repositories;

public interface IUserStoreRepository : IMongoRepository<User>
{
    Task DeleteAsync(Guid userId);
    
    /// <summary>
    /// Returns the active user based on userId passed
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<User> GetUserAsync(Guid userId);
}

public class UserStoreRepository : MongoRepository<User>, IUserStoreRepository
{
    public UserStoreRepository(IMongoClient mongoClient, IDatabaseSettings settings)
        : base(mongoClient, settings)
    {
    }
    
    public async Task DeleteAsync(Guid userId)
    {
        var existingUser = await GetUserAsync(userId);
        if (existingUser == null)
        {
            // no user found skip operation
            return;
        }

        existingUser = existingUser with
        {
            IsActive = false
        };

        // check for password is encrypted or not (will it be responsibility of service?)
        await UpdateAsync(existingUser);
    }

    public async Task<User> GetUserAsync(Guid userId)
    {
        return await GetAsync(u => u.Id == userId && u.IsActive);
    }
}