using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SharedKernel.Infrastructure.Data;


/// <summary>
/// Generic migration service that runs database migrations for a specific DbContext on startup.
/// </summary>
public class MigrationService<TContext> : IHostedService 
    where TContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MigrationService<TContext>> _logger;

    public MigrationService(IServiceProvider serviceProvider, ILogger<MigrationService<TContext>> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        try
        {
            var contextName = typeof(TContext).Name;
            _logger.LogInformation("Starting {ContextName} database migration...", contextName);
            await context.Database.MigrateAsync(cancellationToken);
            _logger.LogInformation("{ContextName} database migration completed successfully", contextName);
        }
        catch (Exception ex)
        {
            var contextName = typeof(TContext).Name;
            _logger.LogError(ex, "Error occurred during {ContextName} database migration", contextName);
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}