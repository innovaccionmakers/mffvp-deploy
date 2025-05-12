using Activations.Application.Abstractions.Data;
using Activations.Application.Abstractions.Lookups;
using Activations.Application.Abstractions.Rules;
using Activations.Domain.Affiliates;
using Activations.Domain.Clients;
using Activations.Domain.MeetsPensionRequirements;
using Activations.Infrastructure.Database;
using Activations.Infrastructure.Mocks;
using Activations.Infrastructure.RulesEngine;
using Common.SharedKernel.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Activations.Infrastructure;

public static class ActivationsModule
{
    public static IServiceCollection AddActivationsModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine(opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ActivationsDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    configuration.GetConnectionString("ActivationsDatabase"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Activations)
                ).UseSnakeCaseNamingConvention();
        });

        services.AddScoped<IAffiliateRepository, AffiliateRepository>();
        services.AddScoped<IMeetsPensionRequirementRepository, MeetsPensionRequirementRepository>();
        services.AddScoped<IClientRepository, InMemoryClientRepository>();
        services.AddSingleton<IErrorCatalog, InMemoryActivationErrorCatalog>();
        services.AddScoped<ILookupService, InMemoryActivationLookupService>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ActivationsDbContext>());
    }
}