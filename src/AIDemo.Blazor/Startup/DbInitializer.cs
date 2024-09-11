using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace AIDemo.Web.Startup;

public abstract class DbInitializer<TDbContext>(
    IServiceProvider serviceProvider,
    ILogger<DbInitializer<TDbContext>> logger)
    : DbInitializer where TDbContext : DbContext
{
    const string ActivitySourceName = "Migrations";

    readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TDbContext>();

        await InitializeDatabaseAsync(dbContext, stoppingToken);
    }

    public override void Dispose()
    {
        _activitySource.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    async Task InitializeDatabaseAsync(TDbContext dbContext, CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity(ActivityKind.Client);

        var sw = Stopwatch.StartNew();

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(dbContext.Database.MigrateAsync, cancellationToken);

        logger.SeedingDatabase();

        await SeedDatabaseAsync(dbContext, cancellationToken);

        logger.InitializationCompleted(sw.ElapsedMilliseconds);
    }

    protected abstract Task SeedDatabaseAsync(TDbContext dbContext, CancellationToken cancellationToken);
}

public abstract class DbInitializer : BackgroundService; // This is here for logging purposes.

public static class DbInitializerStartupExtensions
{
    public static IHostApplicationBuilder AddDbInitializationServices<TDbInitializer, TDbContext>(
        this IHostApplicationBuilder builder)
        where TDbInitializer : DbInitializer<TDbContext>
        where TDbContext : DbContext
    {
        var services = builder.Services;

        services.AddSingleton<TDbInitializer>()
            .AddHostedService(sp => sp.GetRequiredService<TDbInitializer>())
            .AddHealthChecks()
            .AddCheck<DbInitializerHealthCheck>("DbInitializer", null);

        return builder;
    }
}

static partial class DbInitializerLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Database initialization completed after {ElapsedMilliseconds}ms")]
    public static partial void InitializationCompleted(this ILogger<DbInitializer> logger, long elapsedMilliseconds);

    [LoggerMessage(
        EventId = 2,
        Level = LogLevel.Information,
        Message = "Seeding database")]
    public static partial void SeedingDatabase(this ILogger<DbInitializer> logger);
}