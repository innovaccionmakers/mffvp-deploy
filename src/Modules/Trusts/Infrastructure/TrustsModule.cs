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
using Trusts.Application.Abstractions;
using Trusts.Application.Abstractions.Data;

using Trusts.Domain.ConfigurationParameters;
using Trusts.Domain.Trusts;
using Trusts.Infrastructure.ConfigurationParameters;
using Trusts.Infrastructure.Database;

using Trusts.Infrastructure.Trusts;
using Trusts.IntegrationEvents.CreateTrustRequested;
using Trusts.IntegrationEvents.GetBalances;
using Trusts.IntegrationEvents.ObjectiveTrustValidation;
using Trusts.IntegrationEvents.Trusts.PutTrust;
using Trusts.IntegrationEvents.TrustInfo;
using Trusts.IntegrationEvents.TrustYields;
using Trusts.Presentation.GraphQL;
using Trusts.Presentation.MinimalApis;

namespace Trusts.Infrastructure;

public class TrustsModule : IModuleConfiguration
{
    public string ModuleName => "Trusts";
    public string RoutePrefix => "api/trusts";


    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("TrustsDatabase");

        if (env != "Development")
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
        services.AddScoped<IConfigurationParameterLookupRepository<TrustsModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<TrustsModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());
        services.AddScoped<IErrorCatalog<TrustsModuleMarker>, ErrorCatalog<TrustsModuleMarker>>();
        services.AddScoped<CreateTrustRequestedConsumer>();
        services.AddScoped<IRpcHandler<ValidateObjectiveTrustRequest, ValidateObjectiveTrustResponse>, ValidateObjectiveTrustConsumer>();
        services.AddTransient<IRpcHandler<GetBalancesRequest, GetBalancesResponse>, GetBalancesConsumer>();
        services.AddTransient<IRpcHandler<TrustInfoRequest, TrustInfoResponse>, TrustInfoConsumer>();

        services.AddTransient<IRpcHandler<GetTrustParticipantRequest, GetTrustParticipantResponse>, GetTrustParticipantConsumer>();
        services.AddScoped<ITrustExperienceQueries, TrustExperienceQueries>();
        services.AddTransient<IRpcHandler<UpdateTrustFromYieldRequest, UpdateTrustFromYieldResponse>, UpdateTrustFromYieldConsumer>();
        services.AddTransient<IRpcHandler<PutTrustRequest, PutTrustResponse>, PutTrustConsumer>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TrustsDbContext>());

        services.AddRulesEngine<TrustsModuleMarker>(typeof(TrustsModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (app is WebApplication webApp)
        {
            webApp.MapTrustsBusinessEndpoints();
        }
    }
}