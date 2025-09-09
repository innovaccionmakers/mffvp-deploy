using Closing.Application.Abstractions;
using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Domain.ClientOperations;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Closing.Domain.TrustYields;
using Closing.Infrastructure.ClientOperations;
using Closing.Infrastructure.Configuration;
using Closing.Infrastructure.ConfigurationParameters;
using Closing.Infrastructure.Database;
using Closing.Infrastructure.External.Portfolios;
using Closing.Infrastructure.ProfitLossConcepts;
using Closing.Infrastructure.ProfitLosses;
using Closing.Infrastructure.TrustYields;
using Closing.IntegrationEvents.DataSync.CreateClientOperationRequested;
using Closing.IntegrationEvents.DataSync.TrustSync;
using Closing.IntegrationEvents.PortfolioValuation;
using Closing.Integrations.PortfolioValuation;
using Closing.Integrations.PortfolioValuations.Response;
using Closing.Presentation.GraphQL;
using Closing.Presentation.MinimalApis;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Products.IntegrationEvents.PortfolioValuation;

namespace Closing.Infrastructure;

public class ClosingModule : IModuleConfiguration
{
    public string ModuleName => "Closing";
    public string RoutePrefix => "api/closing";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRulesEngine<ClosingModuleMarker>(typeof(ClosingModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("OperationsDatabase");

        if (env != "Development")
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }

        //services.AddDbContext<ClosingDbContext>((sp, options) =>
        //{
        //    options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
        //        .UseNpgsql(
        //            connectionString,
        //            npgsqlOptions =>
        //                npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Closing)
        //        );
        //});

        // Web/API (1 DbContext por request con tracking)
        services.AddDbContextPool<ClosingDbContext>(options =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                   .UseNpgsql(connectionString, npgsqlOptions =>
                       npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Closing));
        });

        // Workers/Paralelo (crear contextos a demanda)
        services.AddPooledDbContextFactory<ClosingDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Closing)
                );
        });

        services.AddScoped<IProfitLossConceptRepository, ProfitLossConceptRepository>();
        services.AddScoped<IProfitLossRepository, ProfitLossRepository>();
        services.AddScoped<IClientOperationRepository, ClientOperationRepository>();
        services.AddScoped<ITrustYieldRepository, TrustYieldRepository>();
        services.AddScoped<IErrorCatalog<ClosingModuleMarker>, ErrorCatalog<ClosingModuleMarker>>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IClosingExperienceQueries, ClosingExperienceQueries>();
        services.AddScoped<IClosingExperienceMutations, ClosingExperienceMutations>();
        services.AddScoped<IConfigurationParameterLookupRepository<ClosingModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<ClosingModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());


        services.AddScoped<IPortfolioValidator, PortfolioValidator>();

        services.AddScoped<IClosingStepEventPublisher, ClosingStepEventPublisher>();

        services.AddScoped<CreateClientOperationRequestedConsumer>();
        services.AddScoped<IRpcHandler<TrustSyncRequest, TrustSyncResponse>, TrustSyncConsumer>();
        services.AddScoped<IRpcHandler<CheckPortfolioValuationExistsRequest, CheckPortfolioValuationExistsResponse>, CheckPortfolioValuationExistsConsumer>();
        services.AddScoped<IRpcHandler<GetPortfolioValuationRequest, GetPortfolioValuationResponse>, GetPortfolioValuationConsumer>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ClosingDbContext>());
        // Llama a la extension para PreClosing
        services.AddPreClosingInfrastructure();
        // Llama a la extension para Closing
        services.AddClosingInfrastructure();

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (app is WebApplication webApp)
        {
            webApp.MapClosingBusinessEndpoints();
        }
    }
}