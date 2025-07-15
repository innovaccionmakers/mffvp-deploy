using Microsoft.Extensions.DependencyInjection;
using Closing.Application.PreClosing.Services.ProfitAndLossConsolidation;
using Closing.Application.PreClosing.Services.Orchestation;
using Closing.Domain.YieldDetails;
using Closing.Infrastructure.YieldDetails;
using Closing.Application.Abstractions.External.Commissions;
using Closing.Infrastructure.External.Commissions;
using Closing.Application.PreClosing.Services.CommissionCalculation;
using Closing.Domain.PortfolioValuations;
using Closing.Infrastructure.PortfolioValuations;
using Closing.Application.PreClosing.Services.TreasuryConcepts;
using Closing.Application.Abstractions.External.TreasuryMovements;
using Closing.Infrastructure.External.TreasuryMovements;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Application.PreClosing.Services.Yield.Builders;
using Closing.Domain.Yields;
using Closing.Infrastructure.Yields;

namespace Closing.Infrastructure.Configuration
{
    public static class PreClosingInfrastructureServices
    {
        public static IServiceCollection AddPreClosingInfrastructure(this IServiceCollection services)
        {
            services.AddScoped<IProfitAndLossConsolidationService, ProfitAndLossConsolidationService>();
            services.AddScoped<IYieldDetailCreationService, YieldDetailCreationService>();
            services.AddScoped<IYieldDetailRepository, YieldDetailRepository>();
            services.AddScoped<IPortfolioValuationRepository, PortfolioValuationRepository>();
            services.AddScoped<IYieldPersistenceService, YieldPersistenceService>();
            services.AddScoped<IYieldRepository, YieldRepository>();
            services.AddScoped<ISimulationOrchestrator, SimulationOrchestrator>();
            services.AddScoped<ICommissionLocator, CommissionLocator>();
            services.AddScoped<ICommissionAdminCalculation, CommissionAdminCalculation>();
            services.AddScoped<ICommissionCalculationService, CommissionCalculationService>();
            services.AddScoped<IMovementsConsolidationService, MovementsConsolidationService>();
            services.AddScoped<ITreasuryMovementsLocator, TreasuryMovementsLocator>();
            

            //TODO: Consultar si puedo agregar Scrutor a este proyecto para poder usar Scan
            //services.Scan(scan => scan
            //.FromAssemblyOf<IYieldDetailBuilder>()
            //.AddClasses(classes => classes.AssignableTo<IYieldDetailBuilder>())
            //.AsImplementedInterfaces()
            //.WithScopedLifetime());

            services.AddScoped<IYieldDetailBuilder, CommissionYieldDetailBuilder>();
            services.AddScoped<IYieldDetailBuilder, ProfitLossYieldDetailBuilder>();
            services.AddScoped<IYieldDetailBuilder, TreasuryYieldDetailBuilder>();

            services.AddScoped<YieldDetailBuilderService>();

            return services;
        }
    }
}
