using WC.Library.Data.Models;

namespace WC.Library.Data.Repository;

public interface IRepository<TEntity> where TEntity : class, IEntity
{
    Task<IEnumerable<TEntity>> Get(bool withIncludes = false, CancellationToken cancellationToken = default);
    Task<TEntity> Create(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity?> GetOneById(Guid id, bool withIncludes = false, CancellationToken cancellationToken = default);
    Task<TEntity> Update(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> Delete(Guid id, CancellationToken cancellationToken = default);
}