using System.Data.Common;

namespace WC.Library.Data.Services;

public interface IWcTransaction
    : IDisposable,
        IAsyncDisposable
{
    Task Commit(
        CancellationToken cancellationToken = default);

    Task Rollback(
        CancellationToken cancellationToken = default);

    public DbTransaction GetDbTransaction();
}
