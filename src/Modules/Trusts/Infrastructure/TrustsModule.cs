using Trusts.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trusts.Application.Abstractions;
using Trusts.Application.Abstractions.Data;
using Trusts.Application.Abstractions.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Trusts.Domain.Trusts;
using Trusts.Infrastructure.ConfigurationParameters;
using Trusts.Infrastructure.Database;
using Trusts.Infrastructure.RulesEngine;
using Trusts.Infrastructure.Trusts;
using Trusts.IntegrationEvents.CreateTrust;

namespace Trusts.Infrastructure;

public static class TrustsModule
{
    public static IServiceCollection AddTrustsModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine<TrustsModuleMarker>(opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TrustsDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    configuration.GetConnectionString("TrustsDatabase"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Trusts)
                );
        });

        services.AddScoped<ITrustRepository, TrustRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IErrorCatalog, ErrorCatalog>();
        services.AddScoped<CreateTrustConsumer>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TrustsDbContext>());
    }
}