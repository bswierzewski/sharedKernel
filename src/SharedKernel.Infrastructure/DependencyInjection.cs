using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Domain.Interfaces;
using SharedKernel.Infrastructure.Interceptors;
using SharedKernel.Infrastructure.Repositories;
using SharedKernel.Repositories;

namespace SharedKernel.Infrastructure;

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
    /// Adds repository and unit of work registrations for a specific DbContext.
    /// Uses explicit typing to ensure type safety and prevent conflicts between multiple DbContext types.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddRepositories<TContext>(this IServiceCollection services) 
        where TContext : DbContext
    {
        services.AddScoped<DbContext>(provider => provider.GetRequiredService<TContext>());
        services.AddScoped<IUnitOfWork, UnitOfWork<TContext>>();
        services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));

        return services;
    }

    /// <summary>
    /// Configures DbContext with SharedKernel interceptors
    /// </summary>
    /// <param name="optionsBuilder">DbContext options builder</param>
    /// <param name="serviceProvider">Service provider to resolve interceptors</param>
    public static void AddInterceptors(this DbContextOptionsBuilder optionsBuilder, IServiceProvider serviceProvider)
    {
        optionsBuilder.AddInterceptors(
            serviceProvider.GetRequiredService<AuditableEntityInterceptor>(),
            serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>()
        );
    }
}