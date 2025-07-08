using Microsoft.Extensions.DependencyInjection;
using Closing.Application.PreClosing.Services.ProfitAndLossConsolidation;
using Closing.Application.PreClosing.Services.YieldDetailCreation;
using Closing.Application.PreClosing.Services.Orchestation;
using Closing.Domain.YieldDetails;
using Closing.Infrastructure.YieldDetails;
using Closing.Application.Abstractions.External.Commissions;
using Closing.Infrastructure.External.Commissions;
using Closing.Application.PreClosing.Services.CommissionCalculation;

namespace Closing.Infrastructure.Configuration
{
    public static class PreClosingInfrastructureServices
    {
        public static IServiceCollection AddPreClosingInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IProfitAndLossConsolidationService, ProfitAndLossConsolidationService>();
            services.AddScoped<IYieldDetailCreationService, YieldDetailCreationService>();
            services.AddScoped<IYieldDetailRepository, YieldDetailRepository>();
            services.AddScoped<ISimulationOrchestrator, SimulationOrchestrator>();
            services.AddScoped<ICommissionLocator, CommissionLocator>();
            services.AddScoped<ICommissionCalculationService, CommissionCalculationService>();
            return services;
        }
    }
}
