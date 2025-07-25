﻿using Closing.Application.Abstractions.External.Operations.SubtransactionTypes;
using Closing.Application.Abstractions.External.Trusts.Trusts;
using Closing.Application.Closing.Services.Abort;
using Closing.Application.Closing.Services.Orchestation;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.Orchestration;
using Closing.Application.Closing.Services.PortfolioValuation;
using Closing.Application.Closing.Services.SubtransactionTypes;
using Closing.Application.Closing.Services.TimeControl;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustSync;
using Closing.Application.Closing.Services.TrustYieldsDistribution;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Infrastructure.External.Operations.SubtransactionTypes;
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
            services.AddScoped<IAbortClosingService, AbortClosingService>();
            services.AddScoped<ISubtransactionTypesLocator, SubtransactionTypesLocator>();
           
            services.AddScoped<ISubtransactionTypesService>(sp =>
            {
                var locator = sp.GetRequiredService<ISubtransactionTypesLocator>();
                var cache = sp.GetRequiredService<IDistributedCache>();
                var logger = sp.GetRequiredService<ILogger<CachedSubtransactionTypesService>>();

                return new CachedSubtransactionTypesService(locator, cache, logger);
            });
            services.AddScoped<IPortfolioValuationService, PortfolioValuationService>();

            services.AddScoped<IDistributeTrustYieldsService, DistributeTrustYieldsService>();
            services.AddScoped<IConfirmClosingOrchestrator, ConfirmClosingOrchestrator>();
            services.AddScoped<ICancelClosingOrchestrator, CancelClosingOrchestrator>();
            services.AddScoped<IDataSyncService, TrustSyncService>();
            services.AddScoped<ITrustLocator, TrustLocator>();

            services.AddScoped<IValidateTrustYieldsDistributionService, ValidateTrustYieldsDistributionService>();

            return services;
        }
    }
}
