using Haack.AIDemoWeb.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haack.AIDemoWeb.Startup;

/// <summary>
/// Service used to run EF Core database migrations.
/// </summary>
public class Migrator(AIDemoContext db)
{
    /// <summary>
    /// Apply database migrations.
    /// </summary>
    public async Task ApplyMigrationsAsync()
    {
        var pending = (await db.Database.GetPendingMigrationsAsync()).ToList();
        if (pending.Count is 0)
        {
            return;
        }

        await db.Database.MigrateAsync();
    }
}

/// <summary>
/// Runs migrations on startup as a background service.
/// </summary>
public class MigratorService : IHostedService
{
    readonly IServiceScopeFactory _scopeFactory;
    readonly ILogger<MigratorService> _logger;

    public MigratorService(IServiceScopeFactory scopeFactory, ILogger<MigratorService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var migrator = scope.ServiceProvider.GetRequiredService<Migrator>();
        _logger.ApplyingDatabaseMigrations();

        await migrator.ApplyMigrationsAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public static partial class MigratorServiceLoggingExtensions
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Applying database migrations…")]
    public static partial void ApplyingDatabaseMigrations(this ILogger<MigratorService> logger);
}

public static class MigrationStartupExtensions
{
    public static IServiceCollection AddMigrationServices(this IServiceCollection services)
    {
        services.AddTransient<Migrator>();
        services.AddHostedService<MigratorService>();

        return services;
    }
}