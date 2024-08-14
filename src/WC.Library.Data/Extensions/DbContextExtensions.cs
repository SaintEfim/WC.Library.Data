using Microsoft.EntityFrameworkCore;
using WC.Library.Data.Services;

namespace WC.Library.Data.Extensions;

public static class DbContextExtensions
{
    public static async Task UseTransaction(
        this DbContext context,
        IWcTransaction transaction,
        CancellationToken cancellationToken)
    {
        await context.Database.UseTransactionAsync(transaction.GetDbTransaction(), cancellationToken);
    }

    public static async Task<IWcTransaction> StartTransaction(
        this DbContext context,
        CancellationToken cancellationToken = default)
    {
        var dbContextTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
        return new WcTransaction(dbContextTransaction);
    }
}
