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

using Operations.Application.Abstractions;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.Cleanup;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Application.Abstractions.Services.TrustCreation;
using Operations.Application.Abstractions.Services.Closing;
using Operations.Application.Abstractions.Services.QueueTransactions;
using Operations.Application.Contributions.Prevalidation;
using Operations.Application.Contributions.Services;
using Operations.Application.Contributions.Services.ClosingValidator;
using Operations.Application.Contributions.Services.Cleanup;
using Operations.Application.Contributions.Services.OperationCompleted;
using Operations.Application.Contributions.Services.TrustCreation;
using Operations.Application.Contributions.Services.QueueTransactions;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.Banks;
using Operations.Domain.Channels;
using Operations.Domain.ClientOperations;
using Operations.Domain.TemporaryClientOperations;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.Origins;
using Operations.Domain.Services;
using Operations.Domain.SubtransactionTypes;
using Operations.Infrastructure.AuxiliaryInformations;
using Operations.Infrastructure.Banks;
using Operations.Infrastructure.Channels;
using Operations.Infrastructure.ClientOperations;
using Operations.Infrastructure.TemporaryClientOperations;
using Operations.Infrastructure.TemporaryAuxiliaryInformations;
using Operations.IntegrationEvents.PendingContributionProcessor;
using Operations.Infrastructure.ConfigurationParameters;
using Operations.Infrastructure.Database;
using Operations.Infrastructure.External.Activate;
using Operations.Infrastructure.External.ContributionValidation;
using Operations.Infrastructure.External.Customers;
using Operations.Infrastructure.Origins;
using Operations.Infrastructure.Services;
using Operations.Infrastructure.SubtransactionTypes;
using Operations.Presentation.GraphQL;
using Operations.Presentation.MinimalApis;
using Operations.Application.Contributions.TransactionControl;

namespace Operations.Infrastructure;

public class OperationsModule: IModuleConfiguration
{
    public string ModuleName => "Operations";
    public string RoutePrefix => "api/operations";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddRulesEngine<OperationsModuleMarker>(typeof(OperationsModule).Assembly, opt =>
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

        services.AddDbContext<OperationsDbContext>((sp, options) =>
        {
            options.ReplaceService<IHistoryRepository, NonLockingNpgsqlHistoryRepository>()
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Operations)
                );
        });

        services.AddScoped<IClientOperationRepository, ClientOperationRepository>();
        services.AddScoped<IAuxiliaryInformationRepository, AuxiliaryInformationRepository>();
        services.AddScoped<ITemporaryClientOperationRepository, TemporaryClientOperationRepository>();
        services.AddScoped<ITemporaryAuxiliaryInformationRepository, TemporaryAuxiliaryInformationRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IConfigurationParameterLookupRepository<OperationsModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<OperationsModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());
        services.AddScoped<IOriginRepository, OriginRepository>();
        services.AddScoped<ISubtransactionTypeRepository, SubtransactionTypeRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<IBankRepository, BankRepository>();
        services.AddScoped<IOperationsExperienceQueries, OperationsExperienceQueries>();
        services.AddScoped<IOperationsExperienceMutation, OperationsExperienceMutation>();

        services.AddScoped<IActivateLocator, ActivateLocator>();
        services.AddScoped<IContributionRemoteValidator, ContributionRemoteValidator>();
        services.AddScoped<IPersonValidator, PersonValidator>();

        services.AddScoped<IContributionCatalogResolver, ContributionCatalogResolver>();
        services.AddScoped<ITaxCalculator, TaxCalculator>();

        services.AddScoped<IQueueTransactions, QueueTransactions>();
        services.AddScoped<IClosingValidator, ClosingValidator>();

        services.AddScoped<IPrevalidate, Prevalidate>();
        services.AddScoped<ITransactionControl, TransactionControl>();
        services.AddScoped<ITrustCreation, TrustCreation>();
        services.AddScoped<IOperationCompleted, OperationCompleted>();
        services.AddScoped<PendingContributionProcessor>();
        services.AddSingleton<ITempClientOperationsCleanupService, TempClientOpsCleanupService>();

        services.AddScoped<IErrorCatalog<OperationsModuleMarker>, ErrorCatalog<OperationsModuleMarker>>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OperationsDbContext>());

        services.AddScoped<ISalesUserService, SalesUserService>();
        services.AddScoped<IChannelService, ChannelService>();
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<IBuildMissingFieldsContributionService, BuildMissingFieldsContributionService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();

        if (app is WebApplication webApp)
        {
            webApp.MapOperationsBusinessEndpoints();
        }
    }
}