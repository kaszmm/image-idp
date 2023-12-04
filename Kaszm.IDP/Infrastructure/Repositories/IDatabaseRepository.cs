using System.Linq.Expressions;
using IdentityServer.Models;

namespace IdentityServer.Infrastructure.Repositories;

public interface IDatabaseRepository
{
}

public interface IDatabaseRepository<TModel> : IDatabaseRepository where TModel : BaseEntity
{
    Task CreateAsync(TModel data);

    Task UpdateAsync(TModel data);

    Task<TModel> GetAsync(Expression<Func<TModel, bool>> where);
}