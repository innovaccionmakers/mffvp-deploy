using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.Data;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.AccountingAssistants;
using Accounting.Domain.Concepts;
using Accounting.Domain.ConfigurationParameters;
using Accounting.Domain.PassiveTransactions;
using Accounting.Domain.Treasuries;
using Accounting.Infrastructure.AccountingAssistants;
using Accounting.Infrastructure.Concepts;
using Accounting.Infrastructure.ConfigurationParameters;
using Accounting.Infrastructure.Database;
using Accounting.Infrastructure.External.Operations;
using Accounting.Infrastructure.External.Portfolios;
using Accounting.Infrastructure.External.Yields;
using Accounting.Infrastructure.PassiveTransactions;
using Accounting.Infrastructure.Treasuries;
using Accounting.Presentation.GraphQL;
using Accounting.Presentation.MinimalApis;
using Common.SharedKernel.Application.Abstractions;
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
using Accounting.Application;

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
        services.AddScoped<IPortfolioLocator, PortfolioLocator>();
        services.AddScoped<IOperationLocator, OperationLocator>();
        services.AddScoped<IAccountingExperienceQueries, AccountingExperienceQueries>();
        services.AddScoped<IPassiveTransactionRepository, PassiveTransactionRepository>();
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AccountingDbContext>());
        services.AddScoped<IAccountingAssistantRepository, AccountingAssistantRepository>();
        services.AddScoped<IAccountProcessExperienceMutations, AccountProcessExperienceMutations>();
        services.AddScoped<ITreasuryRepository, TreasuryRepository>();
        services.AddScoped<IConceptsRepository, ConceptsRepository>();
        services.AddScoped<IInconsistencyHandler, InconsistencyHandler>();

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (app is WebApplication webApp)
        {
            webApp.MapAccountingBusinessEndpoints();
        }
    }
}
