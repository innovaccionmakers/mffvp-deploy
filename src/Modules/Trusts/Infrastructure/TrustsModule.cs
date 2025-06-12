using Trusts.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trusts.Application.Abstractions;
using Trusts.Application.Abstractions.Data;
using Common.SharedKernel.Application.Rules;
using Trusts.Domain.Trusts;
using Trusts.Infrastructure.ConfigurationParameters;
using Trusts.Infrastructure.Database;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Trusts.Infrastructure.Trusts;
using Trusts.IntegrationEvents.CreateTrust;

namespace Trusts.Infrastructure;

public static class TrustsModule
{
    public static IServiceCollection AddTrustsModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine<TrustsModuleMarker>(typeof(TrustsModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("TrustsDatabase");

        if (env == "DevMakers2")
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }

        services.AddDbContext<TrustsDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Trusts)
                );
        });

        services.AddScoped<ITrustRepository, TrustRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IErrorCatalog<TrustsModuleMarker>, ErrorCatalog>();
        services.AddScoped<CreateTrustConsumer>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TrustsDbContext>());
    }
}