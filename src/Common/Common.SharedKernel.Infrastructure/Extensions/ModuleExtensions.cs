using Common.SharedKernel.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Common.SharedKernel.Infrastructure.Extensions;

public static class ModuleExtensions
{
    public static IServiceCollection AddModulesFromAssembly(
        this IServiceCollection services, 
        Assembly assembly, 
        IConfiguration configuration)
    {
        var moduleTypes = assembly.GetTypes()
            .Where(t => typeof(IModuleConfiguration).IsAssignableFrom(t) && 
                       !t.IsInterface && 
                       !t.IsAbstract)
            .ToList();

        foreach (var moduleType in moduleTypes)
        {
            if (Activator.CreateInstance(moduleType) is IModuleConfiguration moduleInstance)
            {                
                services.AddSingleton(moduleInstance);
                                
                moduleInstance.ConfigureServices(services, configuration);
                
                Console.WriteLine($"Registered module: {moduleInstance.ModuleName} (Route: /{moduleInstance.RoutePrefix})");
            }
        }

        return services;
    }

    public static IServiceCollection AddModule<T>(
        this IServiceCollection services, 
        IConfiguration configuration) 
        where T : class, IModuleConfiguration, new()
    {
        var module = new T();
        services.AddSingleton<IModuleConfiguration>(module);
        module.ConfigureServices(services, configuration);
        
        Console.WriteLine($"✅ Registered module: {module.ModuleName} (Route: /{module.RoutePrefix})");
        
        return services;
    }
} 