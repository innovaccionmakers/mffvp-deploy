using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.Data;
using Operations.Domain.ClientOperations;
using Operations.Infrastructure.ClientOperations;
using Operations.Domain.AuxiliaryInformations;
using Operations.Infrastructure.AuxiliaryInformations;
using Operations.Infrastructure.Database;
using Operations.Domain.Banks;
using Operations.Infrastructure.Banks;
using Common.SharedKernel.Infrastructure.Configuration;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.External;
using Operations.Application.Contributions.Services;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.Channels;
using Operations.Domain.ClientOperations;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.Origins;
using Operations.Domain.SubtransactionTypes;
using Operations.Infrastructure.AuxiliaryInformations;
using Operations.Infrastructure.Channels;
using Operations.Infrastructure.ClientOperations;
using Operations.Infrastructure.ConfigurationParameters;
using Operations.Infrastructure.Database;
using Operations.Infrastructure.External.Activate;
using Operations.Infrastructure.External.ContributionValidation;
using Operations.Infrastructure.External.Customers;
using Operations.Infrastructure.External.Trusts;
using Operations.Infrastructure.Origins;
using Operations.Infrastructure.SubtransactionTypes;
using Operations.Presentation.GraphQL;

namespace Operations.Infrastructure;

public static class OperationsModule
{
    public static IServiceCollection AddOperationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine<OperationsModuleMarker>(typeof(OperationsModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
        return services;
    }

    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("OperationsDatabase");

        if (env != "Development")
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }

        services.AddDbContext<OperationsDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Operations)
                );
        });

        services.AddScoped<IClientOperationRepository, ClientOperationRepository>();
        services.AddScoped<IAuxiliaryInformationRepository, AuxiliaryInformationRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IConfigurationParameterLookupRepository<OperationsModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<OperationsModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());
        services.AddScoped<IOriginRepository, OriginRepository>();
        services.AddScoped<ISubtransactionTypeRepository, SubtransactionTypeRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<IBankRepository, BankRepository>();
        services.AddScoped<IOperationsExperienceQueries, OperationsExperienceQueries>();

        services.AddScoped<IActivateLocator, ActivateLocator>();
        services.AddScoped<IContributionRemoteValidator, ContributionRemoteValidator>();
        services.AddScoped<IPersonValidator, PersonValidator>();
        services.AddScoped<ITrustCreator, TrustCreator>();

        services.AddScoped<IContributionCatalogResolver, ContributionCatalogResolver>();
        services.AddScoped<ITaxCalculator, TaxCalculator>();

        services.AddScoped<IErrorCatalog<OperationsModuleMarker>, ErrorCatalog<OperationsModuleMarker>>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OperationsDbContext>());
    }
}