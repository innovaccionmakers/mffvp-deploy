using Closing.Application.Abstractions.External.Operations.ClientOperations;
using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Closing.Application.Abstractions.External.Operations.TrustOperations;
using Closing.Application.Abstractions.External.Products.AccumulatedCommissions;
using Closing.Application.Abstractions.External.Products.Portfolios;
using Closing.Application.Abstractions.External.Trusts.Trusts;
using Closing.Application.Closing.Services.Abort;
using Closing.Application.Closing.Services.DistributableReturns;
using Closing.Application.Closing.Services.DistributableReturns.Interfaces;
using Closing.Application.Closing.Services.OperationTypes;
using Closing.Application.Closing.Services.Orchestation;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.Orchestration;
using Closing.Application.Closing.Services.PortfolioValuation;
using Closing.Application.Closing.Services.ReturnsOperations;
using Closing.Application.Closing.Services.ReturnsOperations.Interfaces;
using Closing.Application.Closing.Services.Telemetry;
using Closing.Application.Closing.Services.TimeControl;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustSync;
using Closing.Application.Closing.Services.TrustYieldsDistribution;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Application.Closing.Services.Validation;
using Closing.Application.Closing.Services.Validation.Interfaces;
using Closing.Application.Closing.Services.Warnings;
using Closing.Application.PostClosing.Services.DailyDataLoad;
using Closing.Application.PostClosing.Services.Orchestation;
using Closing.Application.PostClosing.Services.PendingTransactions;
using Closing.Application.PostClosing.Services.PortfolioCommission;
using Closing.Application.PostClosing.Services.PortfolioServices;
using Closing.Application.PostClosing.Services.TechnicalSheetEvent;
using Closing.Application.PostClosing.Services.TrustSync;
using Closing.Application.PostClosing.Services.TrustYield;
using Closing.Domain.TrustYields;
using Closing.Domain.YieldsToDistribute;
using Closing.Infrastructure.External.DataSync;
using Closing.Infrastructure.External.Operations.ClientOperations;
using Closing.Infrastructure.External.Operations.OperationTypes;
using Closing.Infrastructure.External.Operations.TrustOperations;
using Closing.Infrastructure.External.Products.Commissions;
using Closing.Infrastructure.External.Products.Portfolios;
using Closing.Infrastructure.External.Trusts.Trusts;
using Closing.Infrastructure.TrustYields;
using Closing.Infrastructure.YieldsToDistribute;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Closing.Infrastructure.Configuration
{
    public static class ClosingInfrastructureServices
    {
        public static IServiceCollection AddClosingInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IPrepareClosingOrchestrator, PrepareClosingOrchestrator>();
            services.AddScoped<IPrepareClosingBusinessValidator, PrepareClosingBusinessValidator>();
            services.AddScoped<ITimeControlService, TimeControlService>();
            services.AddScoped<IClosingStepEventPublisher, ClosingStepEventPublisher>();
            services.AddTransient<ClosingStepEventSuscriber>();
            services.AddScoped<IAbortTrustYieldService, AbortTrustYieldService>();
            services.AddScoped<IAbortClosingService, AbortClosingService>();
            services.AddScoped<IAbortPortfolioValuationService, AbortPortfolioValuationService>();
            services.AddScoped<IAbortSimulationService, AbortSimulationService>();
            services.AddScoped<IOperationTypesLocator, OperationTypesLocator>();
            services.AddScoped<IDistributableReturnsService, DistributableReturnsService>();
            services.AddScoped<IReturnsOperationsService, ReturnsOperationsService>();

            services.AddScoped<IOperationTypesService>(sp =>
            {
                var locator = sp.GetRequiredService<IOperationTypesLocator>();
                var cache = sp.GetRequiredService<IDistributedCache>();
                var logger = sp.GetRequiredService<ILogger<CachedOperationTypesService>>();

                return new CachedOperationTypesService(locator, cache, logger);
            });
            services.AddScoped<IPortfolioValuationService, PortfolioValuationService>();
            services.AddScoped<IDistributeTrustYieldsService, DistributeTrustYieldsService>();
            services.AddScoped<IConfirmClosingOrchestrator, ConfirmClosingOrchestrator>();
            services.AddScoped<ICancelClosingOrchestrator, CancelClosingOrchestrator>();
            services.AddScoped<IDataSyncService, DataSyncTrustService>();

            services.AddScoped<IValidateTrustYieldsDistributionService, ValidateTrustYieldsDistributionService>();

            services.AddScoped<IPostClosingServicesOrchestation, PostClosingServicesOrchestation>();
            services.AddScoped<IPortfolioService, PortfolioService>();
            services.AddScoped<ITechnicalSheetPublisher, TechnicalSheetPublisher>();
            services.AddScoped<ITrustYieldProcessor, TrustYieldProcessor>();
            services.AddScoped<IPortfolioCommissionService, PortfolioCommissionService>();
            services.AddScoped<IPendingTransactionsService, PendingTransactionsService>();
            services.AddScoped<IDataSyncPostService, DataSyncTrustPostService>();
            services.AddScoped<IProcessPendingTransactionsRemote, ProcessPendingTransactionsRemote>();
            services.AddScoped<IUpdatePortfolioFromClosingRemote, UpdatePortfolioFromClosingRemote>();
            services.AddScoped<IGetPortfolioDataRemote, GetPortfolioDataRemote>();
            services.AddScoped<IUpsertTrustYieldOperationsRemote, UpsertTrustYieldOperationsRemote>();
            services.AddScoped<IUpdateTrustRemote, UpdateTrustRemote>();
            services.AddScoped<IUpdateAccumulatedCommissionRemote, UpdateAccumulatedCommissionRemote>();
            services.AddScoped<ITrustYieldBulkRepository, TrustYieldBulkRepository>();
            services.AddScoped<IYieldToDistributeRepository, YieldToDistributeRepository>();
            services.AddScoped<IDailyDataPublisher, DailyDataPublisher>();

            services.AddScoped<IWarningCollector, WarningCollector>();

            services.AddSingleton<IClosingStepTimer, ClosingStepTimerLite>();

            return services;
        }
    }
}
