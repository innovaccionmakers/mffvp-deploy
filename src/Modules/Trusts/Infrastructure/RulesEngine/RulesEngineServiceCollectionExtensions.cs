using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RulesEngine.HelperFunctions;
using RulesEngine.Interfaces;
using RulesEngine.Models;
using Trusts.Application.Abstractions.Rules;

namespace Trusts.Infrastructure.RulesEngine;

public sealed class RulesEngineOptions<TModule>
{
    public int CacheSizeLimitMb { get; set; } = 32;
    public string[] EmbeddedResourceSearchPatterns { get; set; }
        = new[] { ".rules.json" };
}

public static class RulesEngineServiceCollectionExtensions
{
    public static IServiceCollection AddRulesEngine<TModule>(
        this IServiceCollection services,
        Action<RulesEngineOptions<TModule>>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);

        services.AddSingleton<IRulesEngine<TModule>>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<RulesEngineOptions<TModule>>>().Value;
            var logger = sp.GetRequiredService<ILoggerFactory>()
                .CreateLogger($"RulesEngineLoader.{typeof(TModule).Name}");
            
            var assembly = typeof(RulesEngineServiceCollectionExtensions).Assembly;
            
            var workflows = LoadWorkflowsFromEmbeddedResources(opt, logger, assembly);
            var reSettings = new ReSettings
            {
                CacheConfig = new MemCacheConfig
                {
                    SizeLimit = opt.CacheSizeLimitMb * 1024
                }
            };

            return new global::RulesEngine.RulesEngine(workflows, reSettings)
                .AsGeneric<TModule>();
        });

        services.AddScoped<IRuleEvaluator<TModule>, RuleEvaluator<TModule>>();
        
        return services;
    }

    private static Workflow[] LoadWorkflowsFromEmbeddedResources<TModule>(
        RulesEngineOptions<TModule> opt,
        ILogger logger,
        Assembly assembly)
    {
        var resources = assembly.GetManifestResourceNames()
            .Where(name => opt.EmbeddedResourceSearchPatterns
                .Any(p => name.EndsWith(p, StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        var workflows = new List<Workflow>();
        foreach (var res in resources)
        {
            using var s = assembly.GetManifestResourceStream(res)!;
            using var sr = new StreamReader(s);
            var json = sr.ReadToEnd();

            var fileWorkflows = JsonSerializer.Deserialize<Workflow[]>(json)!;
            if (fileWorkflows.Length == 0)
                logger.LogWarning("El recurso {Resource} no contiene workflows válidos.", res);

            workflows.AddRange(fileWorkflows);
            logger.LogInformation("Loaded workflow file {Resource}", res);
        }

        logger.LogInformation("Total workflows loaded: {Count}", workflows.Count);
        return workflows.ToArray();
    }
}