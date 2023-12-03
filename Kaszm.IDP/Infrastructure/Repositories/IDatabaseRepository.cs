using System.Linq.Expressions;

namespace IdentityServer.Infrastructure.Repositories;

public interface IDatabaseRepository
{
}

public interface IDatabaseRepository<TModel> : IDatabaseRepository
{
    Task CreateAsync(TModel data);

    Task UpdateAsync(TModel data, Expression<Func<TModel, bool>> where);

    Task<TModel> GetAsync(Expression<Func<TModel, bool>> where);
}