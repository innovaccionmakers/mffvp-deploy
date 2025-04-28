using Microsoft.Extensions.DependencyInjection;

using System.Reflection;

namespace Common.SharedKernel.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        Assembly[] moduleAssemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(moduleAssemblies);

        });

        return services;
    }
}
