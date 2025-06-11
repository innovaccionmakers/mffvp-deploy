using Operations.Domain.ConfigurationParameters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Operations.Application.Abstractions.Data;
using Operations.Domain.ClientOperations;
using Operations.Infrastructure.ClientOperations;
using Operations.Domain.AuxiliaryInformations;
using Operations.Infrastructure.AuxiliaryInformations;
using Operations.Infrastructure.Database;
using Common.SharedKernel.Infrastructure.Configuration;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.External;
using Common.SharedKernel.Application.Rules;
using Operations.Application.Contributions.Services;
using Operations.Domain.Channels;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Operations.Domain.Origins;
using Operations.Domain.SubtransactionTypes;
using Operations.Infrastructure.Channels;
using Operations.Infrastructure.ConfigurationParameters;
using Operations.Infrastructure.External.Activate;
using Operations.Infrastructure.External.ContributionValidation;
using Operations.Infrastructure.External.People;
using Operations.Infrastructure.External.Trusts;
using Operations.Infrastructure.Origins;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Operations.Infrastructure.SubtransactionTypes;

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
        services.AddDbContext<OperationsDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    configuration.GetConnectionString("OperationsDatabase"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Operations)
                );
        });

        services.AddScoped<IClientOperationRepository, ClientOperationRepository>();
        services.AddScoped<IAuxiliaryInformationRepository, AuxiliaryInformationRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IOriginRepository, OriginRepository>();
        services.AddScoped<ISubtransactionTypeRepository, SubtransactionTypeRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();

        services.AddScoped<IActivateLocator, ActivateLocator>();
        services.AddScoped<IContributionRemoteValidator, ContributionRemoteValidator>();
        services.AddScoped<IPersonValidator, PersonValidator>();
        services.AddScoped<ITrustCreator, TrustCreator>();

        services.AddScoped<IContributionCatalogResolver, ContributionCatalogResolver>();
        services.AddScoped<ITaxCalculator, TaxCalculator>();

        services.AddScoped<IErrorCatalog<OperationsModuleMarker>, ErrorCatalog>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OperationsDbContext>());
    }
}