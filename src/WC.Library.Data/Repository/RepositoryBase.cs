using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WC.Library.Data.Extensions;
using WC.Library.Data.Models;
using WC.Library.Data.Services;
using WC.Library.Shared.Exceptions;

namespace WC.Library.Data.Repository;

public abstract class RepositoryBase<TRepository, TDbContext, TEntity> : IRepository<TEntity>
    where TRepository : class, IRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class, IEntity
{
    protected RepositoryBase(
        TDbContext context,
        ILogger<TRepository> logger)
    {
        Context = context;
        Logger = logger;
    }

    private TDbContext Context { get; }

    private ILogger<TRepository> Logger { get; }

    public virtual async Task<IEnumerable<TEntity>> Get(
        bool withIncludes = false,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (transaction != default)
            {
                await Context.UseTransaction(transaction, cancellationToken);
            }

            var query = BuildBaseQuery(withIncludes);

            return await query.ToListAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error creating entity: {Message}", ex.Message);
            throw;
        }
    }

    public virtual async Task<TEntity> Create(
        TEntity entity,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (transaction != default)
            {
                await Context.UseTransaction(transaction, cancellationToken);
            }

            var createdEntity = await Context.Set<TEntity>()
                .AddAsync(entity, cancellationToken);
            try
            {
                await Context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                // detach entity to prevent
                // "The instance of entity type cannot be tracked because another instance with the key value  is already being tracked."
                createdEntity.State = EntityState.Detached;
            }

            return createdEntity.Entity;
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error creating entity: {Message}", ex.Message);
            throw;
        }
    }

    public virtual async Task<TEntity?> GetOneById(
        Guid id,
        bool withIncludes = false,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default)
    {
        if (Guid.Empty.Equals(id))
        {
            return default;
        }

        if (transaction != default)
        {
            await Context.UseTransaction(transaction, cancellationToken);
        }

        var resultQuery = await BuildBaseQuery(withIncludes)
            .Where(x => x.Id.Equals(id))
            .FirstOrDefaultAsync(cancellationToken);

        if (resultQuery == null)
        {
            throw new NotFoundException($"User with id {id} not found.");
        }

        return resultQuery;
    }

    public virtual async Task<TEntity> Update(
        TEntity entity,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (transaction != default)
            {
                await Context.UseTransaction(transaction, cancellationToken);
            }

            var updatedRowEntry = Context.Set<TEntity>()
                .Update(entity);

            try
            {
                await Context.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                updatedRowEntry.State = EntityState.Detached;
            }

            return updatedRowEntry.Entity;
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error updating entity: {Message}", ex.Message);
            throw;
        }
    }

    public virtual async Task<TEntity> Delete(
        Guid id,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (transaction != default)
            {
                await Context.UseTransaction(transaction, cancellationToken);
            }

            var record = await GetOneById(id, false, transaction, cancellationToken);

            if (record == null)
            {
                throw new NotFoundException($"User with id {id} not found.");
            }

            return await Delete(record, cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error deleting entity: {Message}", ex.Message);
            throw;
        }
    }

    protected virtual async Task<TEntity> Delete(
        TEntity entity,
        CancellationToken cancellationToken = default)
    {
        var deletedEntity = Context.Set<TEntity>()
            .Remove(entity);

        try
        {
            await Context.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            deletedEntity.State = EntityState.Detached;
        }

        return deletedEntity.Entity;
    }

    protected virtual IQueryable<TEntity> FillRelatedRecords(
        IQueryable<TEntity> query)
    {
        return query;
    }

    protected virtual IQueryable<TEntity> BuildBaseQuery(
        bool withIncludes)
    {
        var queryable = Context.Set<TEntity>()
            .AsNoTracking();

        if (withIncludes)
        {
            queryable = FillRelatedRecords(queryable);
        }

        return queryable;
    }
}
