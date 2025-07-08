using Closing.Application.Abstractions;
using Closing.Application.Abstractions.Data;
using Closing.Application.Abstractions.External;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.ProfitLossConcepts;
using Closing.Domain.ProfitLosses;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Closing.Infrastructure.ConfigurationParameters;
using Closing.Infrastructure.ProfitLossConcepts;
using Closing.Infrastructure.Database;
using Closing.Infrastructure.External.Portfolios;
using Closing.Infrastructure.ProfitLosses;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Common.SharedKernel.Application.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Closing.Presentation.MinimalApis;
using Closing.Domain.ClientOperations;
using Closing.Infrastructure.ClientOperations;
using Closing.IntegrationEvents.CreateClientOperationRequested;
using Closing.Infrastructure.Configuration;

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

        services.AddDbContext<ClosingDbContext>((sp, options) =>
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
        services.AddScoped<IErrorCatalog<ClosingModuleMarker>, ErrorCatalog<ClosingModuleMarker>>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IConfigurationParameterLookupRepository<ClosingModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<ClosingModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());


        services.AddScoped<IPortfolioValidator, PortfolioValidator>();

        services.AddScoped<CreateClientOperationRequestedConsumer>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<ClosingDbContext>());
        // Llama a la extensión para PreClosing
        services.AddPreClosingInfrastructure();

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        if (app is WebApplication webApp)
        {
            webApp.MapClosingBusinessEndpoints();
        }
    }
}