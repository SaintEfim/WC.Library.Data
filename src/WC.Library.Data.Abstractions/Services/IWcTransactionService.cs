namespace WC.Library.Data.Services;

public interface IWcTransactionService
{
    Task<TResult> Execute<TResult>(
        Func<IWcTransaction, CancellationToken, Task<TResult>> funcAsync,
        IWcTransaction? transaction = default,
        CancellationToken cancellationToken = default);
}
