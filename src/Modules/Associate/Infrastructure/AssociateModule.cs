using Application.Activates;
using Application.PensionRequirements;
using Associate.Application.Abstractions;
using Associate.Application.Abstractions.Data;
using Associate.Domain.Activates;
using Associate.Domain.ConfigurationParameters;
using Associate.Domain.PensionRequirements;
using Associate.Infrastructure.Database;
using Associate.Infrastructure.PensionRequirements;
using Associate.IntegrationEvents.ActivateValidation;
using Associate.IntegrationEvents.GetActivateIds;
using Associate.IntegrationsEvents.GetActivateIds;
using Associate.Presentation.GraphQL;
using Associate.Presentation.MinimalApis;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Database.Interceptors;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Associate.Infrastructure;

public class ActivatesModule: IModuleConfiguration
{
    public string ModuleName => "Activates";
    public string RoutePrefix => "api/activates";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRulesEngine<AssociateModuleMarker>(typeof(ActivatesModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("AssociateDatabase");

        if (env != "Development")
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }

        services.AddDbContext<AssociateDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .AddInterceptors(sp.GetRequiredService<PreviousStateSaveChangesInterceptor>())
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Associate)
                );
        });

        services.AddScoped<IPreviousStateProvider, PreviousStateProvider>();
        services.AddScoped<PreviousStateSaveChangesInterceptor>();

        services.AddScoped<IActivateRepository, ActivateRepository>();
        services.AddScoped<IPensionRequirementRepository, PensionRequirementRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IAssociatesExperienceQueries, AssociatesExperienceQueries>();
        services.AddScoped<IAssociatesExperienceMutations, AssociatesExperienceMutations>();
        services.AddScoped<IConfigurationParameterLookupRepository<AssociateModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<AssociateModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());
        services.AddScoped<IErrorCatalog<AssociateModuleMarker>, ErrorCatalog<AssociateModuleMarker>>();
        services.AddScoped<PensionRequirementCommandHandlerValidation>();
        services.AddScoped<IRpcHandler<GetActivateIdByIdentificationRequest, GetActivateIdByIdentificationResponse>, ActivateValidationConsumer>();
        services.AddScoped<IRpcHandler<GetIdentificationByActivateIdsRequestEvent, GetIdentificationByActivateIdsResponseEvent>, ActivateByActivateIdsValidationConsumer>();
        services.AddScoped<ActivatesCommandHandlerValidation>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AssociateDbContext>());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (app is WebApplication webApp)
        {
            webApp.MapAssociateBusinessEndpoints();
        }
    }
}