using System.Text.Json;
using Associate.Application.Abstractions.Rules;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RulesEngine.HelperFunctions;
using RulesEngine.Interfaces;
using RulesEngine.Models;

namespace Associate.Infrastructure.RulesEngine;

public sealed class RulesEngineOptions
{
    public int CacheSizeLimitMb { get; set; } = 32;

    public string[] EmbeddedResourceSearchPatterns { get; set; }
        = [".rules.json"];
}

public static class RulesEngineServiceCollectionExtensions
{
    public static IServiceCollection AddRulesEngine(
        this IServiceCollection services,
        Action<RulesEngineOptions>? configure = null)
    {
        if (configure is not null)
            services.Configure(configure);

        services.AddSingleton<IRulesEngine>(sp =>
        {
            var opt = sp.GetRequiredService<IOptions<RulesEngineOptions>>().Value;
            var logger = sp.GetRequiredService<ILoggerFactory>()
                .CreateLogger("RulesEngineLoader");

            var workflows = LoadWorkflowsFromEmbeddedResources(opt, logger);
            var reSettings = new ReSettings
            {
                CacheConfig = new MemCacheConfig { SizeLimit = opt.CacheSizeLimitMb * 1024 }
            };

            return new global::RulesEngine.RulesEngine(workflows, reSettings);
        });

        services.AddSingleton<IRuleEvaluator, RuleEvaluator>();
        return services;
    }

    private static Workflow[] LoadWorkflowsFromEmbeddedResources(
        RulesEngineOptions opt, ILogger logger)
    {
        var thisAssembly = typeof(RulesEngineServiceCollectionExtensions).Assembly;
        var resources = thisAssembly.GetManifestResourceNames()
            .Where(name => opt.EmbeddedResourceSearchPatterns
                .Any(p => name.EndsWith(p,
                    StringComparison.OrdinalIgnoreCase)))
            .ToArray();

        var workflows = new List<Workflow>();
        foreach (var res in resources)
        {
            using var s = thisAssembly.GetManifestResourceStream(res)!;
            using var sr = new StreamReader(s);
            var json = sr.ReadToEnd();
            workflows.AddRange(JsonSerializer.Deserialize<Workflow[]>(json)!);
            logger.LogInformation("Loaded workflow file {Resource}", res);
        }

        logger.LogInformation("Total workflows loaded: {Count}", workflows.Count);
        return workflows.ToArray();
    }
}