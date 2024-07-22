using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace WC.Library.Data.PostgreSql.Context;

public class PostgreSqlDbContextFactoryBase<TDbContext>
    where TDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    protected PostgreSqlDbContextFactoryBase(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    protected virtual string ConnectionString => null!;

    public TDbContext CreateDbContext()
    {
        var connectionString = _configuration.GetConnectionString(ConnectionString);

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options => { options.CommandTimeout(30); });
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();

        return (TDbContext) Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options, _environment)!;
    }
}
