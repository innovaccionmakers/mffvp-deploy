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
using Treasury.Application.Abstractions;
using Treasury.Application.Abstractions.Data;
using Treasury.Application.Abstractions.External;
using Treasury.Domain.BankAccounts;
using Treasury.Domain.ConfigurationParameters;
using Treasury.Domain.Issuers;
using Treasury.Domain.TreasuryConcepts;
using Treasury.Domain.TreasuryMovements;
using Treasury.Infrastructure.BankAccounts;
using Treasury.Infrastructure.ConfigurationParameters;
using Treasury.Infrastructure.Database;
using Treasury.Infrastructure.External.Portfolio;
using Treasury.Infrastructure.External.PortfolioValuation;
using Treasury.Infrastructure.Issuers;
using Treasury.Infrastructure.TreasuryConcepts;
using Treasury.Infrastructure.TreasuryMovements;
using Treasury.IntegrationEvents.Issuers.ValidateCollectionBank;
using Treasury.IntegrationEvents.TreasuryMovements.AccountingConcepts;
using Treasury.IntegrationEvents.TreasuryMovements.TreasuryMovementsByPortfolio;
using Treasury.IntegrationEvents.TreasuryMovements.ValidateExistByPortfolioAndAccountNumber;
using Treasury.Presentation.GraphQL;

namespace Treasury.Infrastructure;

public class TreasuryModule : IModuleConfiguration
{
    public string ModuleName => "Treasury";
    public string RoutePrefix => "api/treasury";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("TreasuryDatabase");

        if (env != "Development")
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }

        services.AddDbContext<TreasuryDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Treasury)
                );
        });

        services.AddRulesEngine<TreasuryModuleMarker>(typeof(TreasuryModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });

        services.AddScoped<IIssuerRepository, IssuerRepository>();
        services.AddScoped<IBankAccountRepository, BankAccountRepository>();
        services.AddScoped<ITreasuryConceptRepository, TreasuryConceptRepository>();
        services.AddScoped<ITreasuryMovementRepository, TreasuryMovementRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IConfigurationParameterLookupRepository<TreasuryModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<TreasuryModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());
        services.AddScoped<IErrorCatalog<TreasuryModuleMarker>, ErrorCatalog<TreasuryModuleMarker>>();
        services.AddScoped<TreasuryMovementsByPortfolioConsumer>();
        services.AddScoped<ValidateCollectionBankConsumer>();
        services.AddScoped<ITreasuryExperienceMutations , TreasuryExperienceMutations>();
        services.AddScoped<IRpcHandler<TreasuryMovementsByPortfolioRequest, TreasuryMovementsByPortfolioResponse>, TreasuryMovementsByPortfolioConsumer>();
        services.AddScoped<IRpcHandler<ValidateExistByPortfolioAndAccountNumberRequest, ValidateExistByPortfolioAndAccountNumberResponse>, ValidateExistByPortfolioAndAccountNumberConsumer>();
        services.AddScoped<IRpcHandler<ValidateCollectionBankRequest, ValidateCollectionBankResponse>, ValidateCollectionBankConsumer>();
        services.AddScoped<IRpcHandler<AccountingConceptsRequestEvent, AccountingConceptsResponseEvent>, AccountingConceptsConsumer>();
        services.AddScoped<ITreasuryExperienceQueries, TreasuryExperienceQueries>();
        services.AddScoped<ITreasuryExperienceQueries, TreasuryExperienceQueries>();        
        services.AddScoped<IPortfolioLocator, PortfolioLocator>();
        services.AddScoped<IPortfolioValuationLocator, PortfolioValuationLocator>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<TreasuryDbContext>());

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (app is WebApplication webApp)
        {
        }
    }
}