using Autofac;
using Sieve.Services;
using WC.Library.Data.Services;

namespace WC.Library.Data;

public sealed class WcLibraryDataModule : Module
{
    protected override void Load(
        ContainerBuilder builder)
    {
        builder.RegisterType<WcTransactionService>()
            .As<IWcTransactionService>();

        builder.RegisterType<SieveProcessor>()
            .As<ISieveProcessor>();
    }
}
