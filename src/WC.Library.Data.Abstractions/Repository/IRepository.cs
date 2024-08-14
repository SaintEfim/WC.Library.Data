using WC.Library.Data.Models;
using WC.Library.Data.Services;

namespace WC.Library.Data.Repository;

public interface IRepository<TEntity>
    where TEntity : class, IEntity
{
    Task<IEnumerable<TEntity>> Get(
        bool withIncludes = false,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default);

    Task<TEntity> Create(
        TEntity entity,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default);

    Task<TEntity?> GetOneById(
        Guid id,
        bool withIncludes = false,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default);

    Task<TEntity> Update(
        TEntity entity,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default);

    Task<TEntity> Delete(
        Guid id,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default);
}
