using Accounting.Application;
using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.Data;
using Accounting.Application.Abstractions.External;
using Accounting.Application.AccountingConcepts;
using Accounting.Application.AccountingGeneration.Reports;
using Accounting.Application.AccountingOperations;
using Accounting.Application.AccountingValidator.Reports;
using Accounting.Application.AccountProcess;
using Accounting.Application.AutomaticConcepts;
using Accounting.Application.Services;
using Accounting.Domain.AccountingAccounts;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.AccountingInconsistencies;
using Accounting.Domain.Concepts;
using Accounting.Domain.ConfigurationGenerals;
using Accounting.Domain.ConfigurationParameters;
using Accounting.Domain.ConsecutiveFiles;
using Accounting.Domain.Consecutives;
using Accounting.Domain.PassiveTransactions;
using Accounting.Domain.Treasuries;
using Accounting.Infrastructure.AccountingAccounts;
using Accounting.Infrastructure.AccountingAssistants;
using Accounting.Infrastructure.AccountingInconsistencies;
using Accounting.Infrastructure.AccountProcess;
using Accounting.Infrastructure.Concepts;
using Accounting.Infrastructure.ConfigurationGenerals;
using Accounting.Infrastructure.ConfigurationParameters;
using Accounting.Infrastructure.ConsecutiveFiles;
using Accounting.Infrastructure.Consecutives;
using Accounting.Infrastructure.Database;
using Accounting.Infrastructure.External.Operations;
using Accounting.Infrastructure.External.Portfolios;
using Accounting.Infrastructure.External.Users;
using Accounting.Infrastructure.External.Yields;
using Accounting.Infrastructure.External.YieldsToDistribute;
using Accounting.Infrastructure.PassiveTransactions;
using Accounting.Infrastructure.Treasuries;
using Accounting.IntegrationEvents.AccountingProcess;
using Accounting.Presentation.GraphQL;
using Accounting.Presentation.MinimalApis;
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
using Products.IntegrationEvents.Portfolio.AreAllPortfoliosClosed;

namespace Accounting.Infrastructure;

public class AccountingModule : IModuleConfiguration
{
    public string ModuleName => "Accounting";

    public string RoutePrefix => "api/accounting";


    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRulesEngine<AccountingModuleMarker>(typeof(AccountingModule).Assembly, opt =>
        {
            opt.CacheSizeLimitMb = 64;
            opt.EmbeddedResourceSearchPatterns = [".rules.json"];
        });

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        string connectionString = configuration.GetConnectionString("AccountingDatabase");

        if (env != "Development")
        {
            var secretName = configuration["AWS:SecretsManager:SecretName"];
            var region = configuration["AWS:SecretsManager:Region"];
            connectionString = SecretsManagerHelper.GetSecretAsync(secretName, region).GetAwaiter().GetResult();
        }

        services.AddDbContext<AccountingDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Accounting)
                );
        });
        services.AddScoped<IErrorCatalog<AccountingModuleMarker>, ErrorCatalog<AccountingModuleMarker>>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IConfigurationParameterLookupRepository<AccountingModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<AccountingModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());
        services.AddScoped<IYieldLocator, YieldLocator>();
        services.AddScoped<IYieldDetailsLocator, YieldDetailLocator>();
        services.AddScoped<IYieldToDistributeLocator, YieldToDistributeLocator>();
        services.AddScoped<IPortfolioLocator, PortfolioLocator>();
        services.AddScoped<IUserLocator, UserLocator>();
        services.AddScoped<IOperationLocator, OperationLocator>();
        services.AddScoped<IPassiveTransactionRepository, PassiveTransactionRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AccountingDbContext>());
        services.AddScoped<IAccountingAssistantRepository, AccountingAssistantRepository>();
        services.AddScoped<IAccountingExperienceMutations, AccountingExperienceMutations>();
        services.AddScoped<ITreasuryRepository, TreasuryRepository>();
        services.AddScoped<IConceptsRepository, ConceptsRepository>();
        services.AddScoped<IGeneralConfigurationRepository, GeneralConfigurationRepository>();
        services.AddScoped<IConsecutiveFileRepository, ConsecutiveFileRepository>();
        services.AddScoped<AccountingOperationsHandlerValidation>();
        services.AddScoped<AccountingConceptsHandlerValidator>();
        services.AddScoped<AutomaticConceptsHandlerValidator>();
        services.AddScoped<IInconsistencyHandler, InconsistencyHandler>();
        services.AddScoped<IAccountingInconsistencyRepository, AccountingInconsistencyRepository>();
        services.AddScoped<IConsecutiveRepository, ConsecutiveRepository>();
        services.AddScoped<IAccountingProcessStore, RedisAccountingProcessStore>();
        services.AddScoped<AccountingInconsistenciesReport>();
        services.AddScoped<AccountingGenerationReport>();
        services.AddScoped<AccountingProcessCompletedIntegrationSuscriber>();
        services.AddScoped<AccountingOperationRequestedSubscriber>();
        services.AddScoped<IAccountingNotificationService, AccountingNotificationService>();
        services.AddScoped<IAccountingAccountRepository, AccountingAccountRepository>();
        services.AddScoped<IAccountingExperienceQueries, AccountingExperienceQueries>();
        services.AddScoped<IPassiveTransactionQueries, PassiveTransactionQueries>();
        services.AddScoped<IPassiveTransactionMutations, PassiveTransactionMutations>();
        services.AddScoped<IConceptMutations, ConceptMutations>();
        services.AddScoped<IConceptQueries, ConceptQueries>();
        services.AddTransient<IRpcHandler<AreAllPortfoliosClosedRequest, AreAllPortfoliosClosedResponse>, AreAllPortfoliosClosedConsumer>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (app is WebApplication webApp)
        {
            webApp.MapAccountingBusinessEndpoints();
        }
    }
}
