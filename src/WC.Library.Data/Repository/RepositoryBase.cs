using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WC.Library.Data.Models;
using WC.Library.Shared.Exceptions;

namespace WC.Library.Data.Repository;

public abstract class RepositoryBase<TRepository, TDbContext, TEntity> : IRepository<TEntity>
    where TRepository : class, IRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class, IEntity
{
    protected RepositoryBase(TDbContext context, ILogger<TRepository> logger)
    {
        Context = context;
        Logger = logger;
    }

    private TDbContext Context { get; }

    private ILogger<TRepository> Logger { get; }

    public virtual async Task<IEnumerable<TEntity>> Get(bool withIncludes = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = BuildBaseQuery(withIncludes);

            return await query.ToListAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error creating entity: {Message}", ex.Message);
            throw;
        }
    }

    public virtual async Task<TEntity> Create(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entity);

            await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error creating entity: {Message}", ex.Message);
            throw;
        }
    }

    public virtual async Task<TEntity?> GetOneById(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(id);

            var res = await Context.Set<TEntity>()
                .FindAsync([id, cancellationToken], cancellationToken: cancellationToken);

            if (res == null)
            {
                throw new NotFoundException($"User with id {id} not found.");
            }

            return res;
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error getting entity: {Message}", ex.Message);
            throw;
        }
    }

    public virtual async Task<TEntity> Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entity);

            Context.Set<TEntity>().Update(entity);
            await Context.SaveChangesAsync(cancellationToken);

            return entity;
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error updating entity: {Message}", ex.Message);
            throw;
        }
    }

    public virtual async Task<TEntity> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(id);

            var entityToDelete = await Context.Set<TEntity>()
                .FindAsync([id, cancellationToken], cancellationToken: cancellationToken);

            if (entityToDelete != null)
            {
                Context.Set<TEntity>().Remove(entityToDelete);
                await Context.SaveChangesAsync(cancellationToken);
            }
            else
            {
                throw new InvalidOperationException($"Entity with ID {id} was not found during deletion.");
            }

            return entityToDelete;
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error deleting entity: {Message}", ex.Message);
            throw;
        }
    }

    protected virtual IQueryable<TEntity> FillRelatedRecords(IQueryable<TEntity> query)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> BuildBaseQuery(bool withIncludes)
    {
        var queryable = Context.Set<TEntity>().AsNoTracking();

        if (withIncludes)
        {
            queryable = FillRelatedRecords(queryable);
        }

        return queryable;
    }
}