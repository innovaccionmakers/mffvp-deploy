using Accounting.Application;
using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.Data;
using Accounting.Application.Abstractions.External;
using Accounting.Application.AccountingConcepts;
using Accounting.Application.AccountingGeneration.Reports;
using Accounting.Application.AccountingOperations;
using Accounting.Application.AccountingValidator.Reports;
using Accounting.Application.AccountProcess;
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
using Accounting.Infrastructure.External.Administrators;
using Accounting.Infrastructure.External.Operations;
using Accounting.Infrastructure.External.Portfolios;
using Accounting.Infrastructure.External.Users;
using Accounting.Infrastructure.External.Yields;
using Accounting.Infrastructure.External.YieldsToDistribute;
using Accounting.Infrastructure.PassiveTransactions;
using Accounting.Infrastructure.Treasuries;
using Accounting.IntegrationEvents.AccountingProcess;
using Accounting.Presentation.ConfigurationGenerals.CreateConfigurationGeneral;
using Accounting.Presentation.ConfigurationGenerals.DeleteConfigurationGeneral;
using Accounting.Presentation.ConfigurationGenerals.UpdateConfigurationGeneral;
using Accounting.Presentation.GraphQL;
using Accounting.Presentation.GraphQL.Inputs;
using Accounting.Presentation.GraphQL.Inputs.PassiveTransactionInput;
using Accounting.Presentation.GraphQL.Inputs.TreasuriesInput;
using Accounting.Presentation.MinimalApis;
using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Database.Interceptors;
using Common.SharedKernel.Infrastructure.RulesEngine;
using FluentValidation;
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
                .AddInterceptors(sp.GetRequiredService<PreviousStateSaveChangesInterceptor>())
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Accounting)
                );
        });
        services.AddScoped<IPreviousStateProvider, PreviousStateProvider>();
        services.AddScoped<PreviousStateSaveChangesInterceptor>();
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
        services.AddScoped<IAdministratorLocator, AdministratorLocator>();
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
        services.AddScoped<IConcecutivesSetup, ConcecutivesSetup>();
        services.AddScoped<IAccountingExperienceQueries, AccountingExperienceQueries>();
        services.AddScoped<IPassiveTransactionExperienceQueries, PassiveTransactionExperienceQueries>();
        services.AddScoped<IPassiveTransactionExperienceMutations, PassiveTransactionExperienceMutations>();
        services.AddScoped<ITreasuriesExperienceQueries, TreasuriesExperienceQueries>();
        services.AddScoped<ITreasuriesExperienceMutations, TreasuriesExperienceMutations>();
        services.AddScoped<IConceptMutations, ConceptMutations>();
        services.AddScoped<IConceptQueries, ConceptQueries>();
        services.AddScoped<IConfigurationGeneralsExperienceQueries, ConfigurationGeneralsExperienceQueries>();
        services.AddScoped<IConfigurationGeneralsExperienceMutations, ConfigurationGeneralsExperienceMutations>();
        services.AddTransient<IValidator<CreateConceptInput>, InputValidator<CreateConceptInput>>();
        services.AddTransient<IValidator<UpdateConceptInput>, InputValidator<UpdateConceptInput>>();
        services.AddTransient<IValidator<DeleteConceptInput>, InputValidator<DeleteConceptInput>>();
        services.AddTransient<IValidator<CreateConfigurationGeneralInput>, InputValidator<CreateConfigurationGeneralInput>>();
        services.AddTransient<IValidator<UpdateConfigurationGeneralInput>, InputValidator<UpdateConfigurationGeneralInput>>();
        services.AddTransient<IValidator<DeleteConfigurationGeneralInput>, InputValidator<DeleteConfigurationGeneralInput>>();
        services.AddTransient<IValidator<CreatePassiveTransactionInput>, InputValidator<CreatePassiveTransactionInput>>();
        services.AddTransient<IValidator<UpdatePassiveTransactionInput>, InputValidator<UpdatePassiveTransactionInput>>();
        services.AddTransient<IValidator<DeletePassiveTransactionInput>, InputValidator<DeletePassiveTransactionInput>>();
        services.AddTransient<IValidator<CreateTreasuryInput>, InputValidator<CreateTreasuryInput>>();
        services.AddTransient<IValidator<UpdateTreasuryInput>, InputValidator<UpdateTreasuryInput>>();
        services.AddTransient<IValidator<DeleteTreasuryInput>, InputValidator<DeleteTreasuryInput>>();
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
