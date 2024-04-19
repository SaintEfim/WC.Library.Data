using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace WC.Library.Data.PostgreSql.Context;

public class PostgreSqlDbContextFactoryBase<TDbContext> where TDbContext : DbContext
{
    private readonly IConfiguration _configuration;

    protected PostgreSqlDbContextFactoryBase(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected virtual string ConnectionString => null!;

    public TDbContext CreateDbContext()
    {
        var connectionString = _configuration.GetConnectionString(ConnectionString);

        var optionsBuilder = new DbContextOptionsBuilder<TDbContext>();
        optionsBuilder.UseNpgsql(connectionString, options => { options.CommandTimeout(30); });
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();

        return (TDbContext)Activator.CreateInstance(typeof(TDbContext), optionsBuilder.Options)!;
    }
}