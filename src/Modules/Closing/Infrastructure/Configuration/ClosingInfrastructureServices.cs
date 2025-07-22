using Closing.Application.Abstractions.External.Operations.SubtransactionTypes;
using Closing.Application.Closing.Services.Orchestation;
using Closing.Application.Closing.Services.Orchestration;
using Closing.Application.Closing.Services.SubtransactionTypes;
using Closing.Application.Closing.Services.TimeControl;
using Closing.Infrastructure.External.Operations.SubtransactionTypes;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Closing.Infrastructure.Configuration
{
    public static class ClosingInfrastructureServices
    {
        public static IServiceCollection AddClosingInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IClosingOrchestrator, ClosingOrchestrator>();
            services.AddScoped<ITimeControlService, TimeControlService>();
            services.AddScoped<ISubtransactionTypesLocator, SubtransactionTypesLocator>();

            services.AddScoped<ISubtransactionTypesService>(sp =>
            {
                var locator = sp.GetRequiredService<ISubtransactionTypesLocator>();
                var cache = sp.GetRequiredService<IDistributedCache>();
                var logger = sp.GetRequiredService<ILogger<CachedSubtransactionTypesService>>();

                return new CachedSubtransactionTypesService(locator, cache, logger);
            });

            return services;
        }
    }
}
