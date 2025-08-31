using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel.Infrastructure.Interceptors;

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