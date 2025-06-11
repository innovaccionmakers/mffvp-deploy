using Associate.Domain.ConfigurationParameters;
using Application.PensionRequirements;
using Associate.Application.Abstractions;
using Associate.Application.Abstractions.Data;
using Common.SharedKernel.Application.Rules;
using Associate.Domain.Activates;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Associate.Domain.PensionRequirements;
using Associate.Infrastructure.Database;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Associate.IntegrationEvents.ActivateValidation;
using Common.SharedKernel.Infrastructure.Configuration;
using Infrastructure.ConfigurationParameters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Associate.Infrastructure;

public static class ActivatesModule
{
    public static IServiceCollection AddActivatesModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);
        services.AddRulesEngine<AssociateModuleMarker>(typeof(ActivatesModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
        return services;
    }


    private static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AssociateDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    configuration.GetConnectionString("AssociateDatabase"),
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Associate)
                );
        });

        services.AddScoped<IActivateRepository, ActivateRepository>();
        services.AddScoped<IPensionRequirementRepository, PensionRequirementRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IErrorCatalog<AssociateModuleMarker>, ErrorCatalog>();
        services.AddScoped<PensionRequirementCommandHandlerValidation>();
        services.AddScoped<ActivateValidationConsumer>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AssociateDbContext>());
    }
}