using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Domain.Abstractions;
using SharedKernel.Infrastructure.Data;
using SharedKernel.Infrastructure.Interceptors;

namespace SharedKernel.Infrastructure;

/// <summary>
/// Builder for registering components for a specific DbContext module.
/// </summary>
public class ModuleContextBuilder<TContext> where TContext : DbContext
{
    public IServiceCollection Services { get; }

    public ModuleContextBuilder(IServiceCollection services)
    {
        Services = services;
        // Each module context gets its own UnitOfWork tied to its specific DbContext.
        Services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
    }

    /// <summary>
    /// Registers a generic repository for the given entity type.
    /// The repository will be correctly scoped to the module's DbContext.
    /// </summary>
    public ModuleContextBuilder<TContext> AddRepository<TEntity>() where TEntity : class, IAggregateRoot
    {
        Services.AddScoped<IRepository<TEntity>>(provider =>
            new BaseRepository<TEntity, TContext>(provider.GetRequiredService<TContext>()));
        return this;
    }

    /// <summary>
    /// Registers a migration service for the module's DbContext.
    /// The service will automatically run migrations on application startup.
    /// </summary>
    public ModuleContextBuilder<TContext> AddMigrations()
    {
        Services.AddHostedService<MigrationService<TContext>>();
        return this;
    }
}

public static class DependencyInjection
{
    public static IServiceCollection AddSharedKernelInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<AuditableEntityInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();
        return services;
    }

    /// <summary>
    /// Creates a registration scope for a specific DbContext, allowing repositories
    /// and a UnitOfWork to be tied to that single context.
    /// </summary>
    public static IServiceCollection AddModuleContext<TContext>(
        this IServiceCollection services,
        Action<ModuleContextBuilder<TContext>> builder)
        where TContext : DbContext
    {
        var moduleBuilder = new ModuleContextBuilder<TContext>(services);
        builder(moduleBuilder);
        return services;
    }

    /// <summary>
    /// Configures DbContext with SharedKernel interceptors
    /// </summary>
    public static void AddInterceptors(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider)
    {
        optionsBuilder.AddInterceptors(
            serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
            serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>()
        );
    }
}