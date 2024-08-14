using System.Data.Common;
using Microsoft.EntityFrameworkCore.Storage;

namespace WC.Library.Data.Services;

internal sealed class WcTransaction : IWcTransaction
{
    private readonly IDbContextTransaction _dbContextTransaction;

    public WcTransaction(
        IDbContextTransaction dbContextTransaction)
    {
        _dbContextTransaction = dbContextTransaction;
    }

    public async Task Commit(
        CancellationToken cancellationToken = default)
    {
        await _dbContextTransaction.CommitAsync(cancellationToken);

        await DisposeAsync();
    }

    public DbTransaction GetDbTransaction()
    {
        return _dbContextTransaction.GetDbTransaction();
    }

    public async Task Rollback(
        CancellationToken cancellationToken = default)
    {
        await _dbContextTransaction.RollbackAsync(cancellationToken);

        await DisposeAsync();
    }

    public void Dispose()
    {
        _dbContextTransaction.Dispose();
    }

    public ValueTask DisposeAsync()
    {
        return _dbContextTransaction.DisposeAsync();
    }
}
