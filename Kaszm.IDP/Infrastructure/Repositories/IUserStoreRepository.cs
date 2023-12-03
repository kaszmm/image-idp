using System.Linq.Expressions;
using IdentityServer.Models;
using MongoDB.Driver;

namespace IdentityServer.Infrastructure.Repositories;

public interface IUserStoreRepository : IMongoRepository<User>
{
    
}

public class UserStoreRepository : MongoRepository<User>, IUserStoreRepository
{
    public UserStoreRepository(IMongoClient mongoClient, IDatabaseSettings settings)
        : base(mongoClient, settings)
    {
    }

    public async Task Create(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        
        // check for password is encrypted or not (will it be responsibility of service?)
        await CreateAsync(user);
    }
    
    public async Task Update(User user)
    {
        ArgumentNullException.ThrowIfNull(user);
        var existingUser = await Get(user.Id);
        if (existingUser == null)
        {
            throw new ArgumentException("User doesnt exist");
        }

        var existingUserWithEmail = await GetAsync(u => u.Email == user.Email
                                                        && u.Id != user.Id);
        if (existingUserWithEmail != null)
        {
            throw new ArgumentException("User with same email already exist");
        }

        // check for password is encrypted or not (will it be responsibility of service?)
        await UpdateAsync(user, u => u.Id == user.Id);
    }
    
    public async Task Delete(Guid userId)
    {
        var existingUser = await Get(userId);
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
        await UpdateAsync(existingUser, u => u.Id == userId);
    }

    public async Task<User> Get(Guid userId)
    {
        return await GetAsync(u => u.Id == userId && u.IsActive);
    }
    
    public async Task<User> Get(Expression<Func<User,bool>> where)
    {
        return await GetAsync(where);
    }
}