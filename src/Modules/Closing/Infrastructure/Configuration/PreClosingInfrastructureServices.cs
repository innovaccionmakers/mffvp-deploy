using Closing.Application.Abstractions.External.Operations.SubtransactionTypes;
using Closing.Application.Abstractions.External.Products.Commissions;
using Closing.Application.Abstractions.External.Treasury.TreasuryMovements;
using Closing.Application.PreClosing.Services.Commission;
using Closing.Application.PreClosing.Services.Commission.Interfaces;
using Closing.Application.PreClosing.Services.Orchestation;
using Closing.Application.PreClosing.Services.ProfitAndLoss;
using Closing.Application.PreClosing.Services.TreasuryConcepts;
using Closing.Application.PreClosing.Services.Validation;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Application.PreClosing.Services.Yield.Builders;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Infrastructure.External.Operations.SubtransactionTypes;
using Closing.Infrastructure.External.Products.Commissions;
using Closing.Infrastructure.External.Treasury.TreasuryMovements;
using Closing.Infrastructure.PortfolioValuations;
using Closing.Infrastructure.YieldDetails;
using Closing.Infrastructure.Yields;
using Closing.Integrations.PreClosing.RunSimulation;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddScoped<IBusinessValidator<RunSimulationCommand>, RunSimulationBusinessValidator>();
            services.AddScoped<ISubtransactionTypesLocator, SubtransactionTypesLocator>();

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
