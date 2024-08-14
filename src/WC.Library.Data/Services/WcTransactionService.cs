using Microsoft.EntityFrameworkCore;

namespace WC.Library.Data.Services;

internal sealed class WcTransactionService : IWcTransactionService
{
    private readonly DbContext _context;

    public WcTransactionService(
        DbContext context)
    {
        _context = context;
    }

    public async Task<TResult> Execute<TResult>(
        Func<IWcTransaction, CancellationToken, Task<TResult>> funcAsync,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default)
    {
        var internalTransaction = false;
        if (transaction == default)
        {
            transaction = new WcTransaction(await _context.Database.BeginTransactionAsync(cancellationToken));
            internalTransaction = true;
        }

        try
        {
            var result = await funcAsync(transaction, cancellationToken);
            if (internalTransaction)
            {
                await transaction.Commit(cancellationToken);
            }

            return result;
        }
        catch
        {
            if (internalTransaction)
            {
                await transaction.Rollback(cancellationToken);
            }

            throw;
        }
    }
}
