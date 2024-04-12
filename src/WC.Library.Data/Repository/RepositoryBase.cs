using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using WC.Library.Shared.Exceptions;

namespace WC.Library.Data.Repository;

public abstract class RepositoryBase<TRepository, TDbContext, TEntity> : IRepository<TEntity>
    where TDbContext : DbContext
    where TEntity : class
{
    protected RepositoryBase(TDbContext context, ILogger<TRepository> logger)
    {
        Context = context;
        Logger = logger;
    }

    private TDbContext Context { get; }

    private ILogger<TRepository> Logger { get; }

    public virtual async Task<ICollection<TEntity>> Get(CancellationToken cancellationToken = default)
    {
        try
        {
            return await Context.Set<TEntity>().ToListAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error creating entity: {Message}", ex.Message);
            throw;
        }
    }

    public virtual async Task Create(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entity);

            await Context.Set<TEntity>().AddAsync(entity, cancellationToken);
            await Context.SaveChangesAsync(cancellationToken);
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

    public virtual async Task Update(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            ArgumentNullException.ThrowIfNull(entity);

            Context.Set<TEntity>().Update(entity);
            await Context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error updating entity: {Message}", ex.Message);
            throw;
        }
    }

    public virtual async Task Delete(Guid id, CancellationToken cancellationToken = default)
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
        }
        catch (DbUpdateException ex)
        {
            Logger.LogError(ex, "Error deleting entity: {Message}", ex.Message);
            throw;
        }
    }
}