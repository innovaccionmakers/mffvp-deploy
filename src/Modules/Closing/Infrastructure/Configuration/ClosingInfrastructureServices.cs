using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Closing.Application.Abstractions.External.Trusts.Trusts;
using Closing.Application.Closing.Services.Abort;
using Closing.Application.Closing.Services.OperationTypes;
using Closing.Application.Closing.Services.Orchestation;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.Orchestration;
using Closing.Application.Closing.Services.PortfolioValuation;
using Closing.Application.Closing.Services.TimeControl;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustSync;
using Closing.Application.Closing.Services.TrustYieldsDistribution;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Application.PostClosing.Services.Orchestation;
using Closing.Application.PostClosing.Services.PendingTransactionHandler;
using Closing.Application.PostClosing.Services.PortfolioCommissionEvent;
using Closing.Application.PostClosing.Services.PortfolioUpdateEvent;
using Closing.Application.PostClosing.Services.TrustSync;
using Closing.Application.PostClosing.Services.TrustYieldEvent;
using Closing.Infrastructure.External.DataSync;
using Closing.Infrastructure.External.Operations.OperationTypes;
using Closing.Infrastructure.External.Trusts.Trusts;
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
            services.AddScoped<ITimeControlService, TimeControlService>();
            services.AddScoped<IClosingStepEventPublisher, ClosingStepEventPublisher>();
            services.AddTransient<ClosingStepEventSuscriber>();
            services.AddScoped<IAbortClosingService, AbortClosingService>();
            services.AddScoped<IOperationTypesLocator, OperationTypesLocator>();
           
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
            services.AddScoped<ITrustLocator, TrustLocator>();

            services.AddScoped<IValidateTrustYieldsDistributionService, ValidateTrustYieldsDistributionService>();

            services.AddScoped<IPostClosingEventsOrchestation, PostClosingEventsOrchestation>();
            services.AddScoped<IPortfolioUpdatePublisher, PortfolioUpdatePublisher>();
            services.AddScoped<ITrustYieldPublisher, TrustYieldPublisher>();
            services.AddScoped<IPortfolioCommissionPublisher, PortfolioCommissionPublisher>();
            services.AddScoped<IPendingTransactionHandler, PendingTransactionHandler>();
            services.AddScoped<IDataSyncPostService, DataSyncTrustPostService>();

            return services;
        }
    }
}
