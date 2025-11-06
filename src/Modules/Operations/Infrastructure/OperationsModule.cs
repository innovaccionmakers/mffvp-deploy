using Common.SharedKernel.Application.Abstractions;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Application.Rules;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Configuration;
using Common.SharedKernel.Infrastructure.ConfigurationParameters;
using Common.SharedKernel.Infrastructure.Database.Interceptors;
using Common.SharedKernel.Infrastructure.RulesEngine;
using Customers.IntegrationEvents.ClientValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Operations.Application.Abstractions;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.Channel;
using Operations.Application.Abstractions.Services.Cleanup;
using Operations.Application.Abstractions.Services.Closing;
using Operations.Application.Abstractions.Services.ContributionService;
using Operations.Application.Abstractions.Services.AccountingRecords;
using Operations.Application.Abstractions.Services.Voids;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Application.Abstractions.Services.Portfolio;
using Operations.Application.Abstractions.Services.Prevalidation;
using Operations.Application.Abstractions.Services.QueueTransactions;
using Operations.Application.Abstractions.Services.SalesUser;
using Operations.Application.Abstractions.Services.TransactionControl;
using Operations.Application.Abstractions.Services.TrustCreation;
using Operations.Application.AccountingRecords.Services;
using Operations.Application.ChannelService;
using Operations.Application.Contributions.Prevalidation;
using Operations.Application.Contributions.Services;
using Operations.Application.Contributions.Services.Cleanup;
using Operations.Application.Contributions.Services.ClosingValidator;
using Operations.Application.Contributions.Services.OperationCompleted;
using Operations.Application.Contributions.Services.QueueTransactions;
using Operations.Application.Contributions.Services.TrustCreation;
using Operations.Application.Contributions.TransactionControl;
using Operations.Application.Voids.Services;
using Operations.Application.Portfolio.Services;
using Operations.Application.SalesUser.Services;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.Channels;
using Operations.Domain.ClientOperations;
using Operations.Domain.ConfigurationParameters;
using Operations.Domain.OperationTypes;
using Operations.Domain.Origins;
using Operations.Domain.TemporaryAuxiliaryInformations;
using Operations.Domain.TemporaryClientOperations;
using Operations.Domain.TrustOperations;
using Operations.Infrastructure.AuxiliaryInformations;
using Operations.Infrastructure.Channels;
using Operations.Infrastructure.ClientOperations;
using Operations.Infrastructure.ConfigurationParameters;
using Operations.Infrastructure.Database;
using Operations.Infrastructure.External.Activate;
using Operations.Infrastructure.External.CollectionBankValidation;
using Operations.Infrastructure.External.ContributionValidation;
using Operations.Infrastructure.External.Customers;
using Operations.Infrastructure.External.Portfolio;
using Operations.Infrastructure.External.PortfolioValuations;
using Operations.Infrastructure.External.Trusts;
using Operations.Infrastructure.OperationTypes;
using Operations.Infrastructure.Origins;
using Operations.Infrastructure.TemporaryAuxiliaryInformations;
using Operations.Infrastructure.TemporaryClientOperations;
using Operations.Infrastructure.TrustOperations;
using Operations.IntegrationEvents.ClientOperations;
using Operations.IntegrationEvents.OperationTypes;
using Operations.IntegrationEvents.PendingContributionProcessor;
using Operations.IntegrationEvents.TrustOperations;
using Operations.Presentation.GraphQL;
using Operations.Presentation.MinimalApis;

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
                .AddInterceptors(sp.GetRequiredService<PreviousStateSaveChangesInterceptor>())
                .UseNpgsql(
                    connectionString,
                    npgsqlOptions =>
                        npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Operations)
                );
        });
        
        services.AddScoped<IPreviousStateProvider, PreviousStateProvider>();
        services.AddScoped<PreviousStateSaveChangesInterceptor>();

        services.AddScoped<IClientOperationRepository, ClientOperationRepository>();
        services.AddScoped<IAuxiliaryInformationRepository, AuxiliaryInformationRepository>();
        services.AddScoped<ITemporaryClientOperationRepository, TemporaryClientOperationRepository>();
        services.AddScoped<ITemporaryAuxiliaryInformationRepository, TemporaryAuxiliaryInformationRepository>();
        services.AddScoped<IConfigurationParameterRepository, ConfigurationParameterRepository>();
        services.AddScoped<IConfigurationParameterLookupRepository<OperationsModuleMarker>>(sp =>
            (IConfigurationParameterLookupRepository<OperationsModuleMarker>)sp.GetRequiredService<IConfigurationParameterRepository>());
        services.AddScoped<IOriginRepository, OriginRepository>();
        services.AddScoped<IOperationTypeRepository, OperationTypeRepository>();
        services.AddScoped<IChannelRepository, ChannelRepository>();
        services.AddScoped<IOperationsExperienceQueries, OperationsExperienceQueries>();
        services.AddScoped<IOperationsExperienceMutation, OperationsExperienceMutation>();

        services.AddScoped<IActivateLocator, ActivateLocator>();
        services.AddScoped<IContributionRemoteValidator, ContributionRemoteValidator>();
        services.AddScoped<ICollectionBankValidator, CollectionBankValidator>();
        services.AddScoped<IPersonValidator, PersonValidator>();

        services.AddScoped<IContributionCatalogResolver, ContributionCatalogResolver>();
        services.AddScoped<ITaxCalculator, TaxCalculator>();

        services.AddScoped<IQueueTransactions, QueueTransactions>();
        services.AddScoped<IClosingValidator, ClosingValidator>();

        services.AddScoped<IAccountingRecordsOper, AccountingRecordsOper>();
        services.AddScoped<IVoidsOper, VoidedTransactionsOper>();

        services.AddScoped<IPrevalidate, Prevalidate>();
        services.AddScoped<ITransactionControl, TransactionControl>();
        services.AddScoped<ITrustCreation, TrustCreation>();
        services.AddScoped<IOperationCompleted, OperationCompleted>();
        services.AddScoped<PendingContributionProcessor>();
        services.AddSingleton<TempClientOpsCleanupService>();
        services.AddSingleton<ITempClientOperationsCleanupService>(sp =>
            sp.GetRequiredService<TempClientOpsCleanupService>());
        services.AddHostedService(sp => sp.GetRequiredService<TempClientOpsCleanupService>());

        services.AddScoped<IErrorCatalog<OperationsModuleMarker>, ErrorCatalog<OperationsModuleMarker>>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<OperationsDbContext>());

        services.AddScoped<ISalesUserService, SalesUserService>();
        services.AddScoped<IChannelService, ChannelService>();
        services.AddScoped<IPortfolioService, PortfolioService>();
        services.AddScoped<IBuildMissingFieldsContributionService, BuildMissingFieldsContributionService>();

        services.AddScoped<ITrustInfoProvider, TrustInfoProvider>();
        services.AddScoped<ITrustDetailsProvider, TrustDetailsProvider>();
        services.AddScoped<ITrustUpdater, TrustUpdater>();
        services.AddScoped<IPortfolioValuationProvider, PortfolioValuationProvider>();

        services.AddScoped<IRpcHandler<GetAllOperationTypesRequest, GetAllOperationTypesResponse>, GetAllOperationTypesConsumer>();
        services.AddScoped<IRpcHandler<GetOperationTypeByNameRequest, GetOperationTypeByNameResponse>, GetOperationTypeByNameConsumer>();
        services.AddTransient<IRpcHandler<GetAccountingOperationsRequestEvents, GetAccountingOperationsValidationResponse>, AccountingOperationsConsumer>();
        services.AddScoped<IRpcHandler<GetTrustOperationsByPortfolioProcessDateAndTypeRequest, GetTrustOperationsByPortfolioProcessDateAndTypeResponse>, GetTrustOperationsByPortfolioProcessDateAndTypeConsumer>();

        services.AddScoped<ITrustOperationRepository, TrustOperationRepository>();

        services.AddScoped<IPortfolioLocator, PortfolioLocator>();

        services.AddTransient<IRpcHandler<CreateTrustYieldOpFromClosingRequest,CreateTrustYieldOpFromClosingResponse>, CreateTrustYieldOpFromClosingConsumer>();

        services.AddTransient<IRpcHandler<ProcessPendingTransactionsRequest, ProcessPendingTransactionsResponse>, PendingContributionProcessor>();

        services.AddScoped<ITrustOperationBulkRepository, TrustOperationBulkRepository>();
        services.AddScoped<IPendingTransactionsReaderRepository, PendingTransactionsReaderRepository>();

    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (app is WebApplication webApp)
        {
            webApp.MapOperationsBusinessEndpoints();
        }
    }
}