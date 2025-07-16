using Closing.Domain.PortfolioValuations;
using Closing.Domain.PreClosing;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.General;

namespace Closing.Application.PreClosing.Services.Yield;

public sealed class YieldPersistenceService : IYieldPersistenceService
{
    private readonly IYieldDetailRepository _yieldDetailRepository;
    private readonly IYieldRepository _yieldRepository;
    private readonly IPortfolioValuationRepository _portfolioValuationRepository;

    public YieldPersistenceService(
        IYieldDetailRepository yieldDetailRepository,
        IYieldRepository yieldRepository,
        IPortfolioValuationRepository portfolioValuationRepository)
    {
        _yieldDetailRepository = yieldDetailRepository;
        _yieldRepository = yieldRepository;
        _portfolioValuationRepository = portfolioValuationRepository;
    }

    public async Task<SimulatedYieldResult> ConsolidateAsync(
        int portfolioId,
        DateTime closingDateLocal,
        bool isClosed,
        CancellationToken ct = default)
    {
        var closingDateUtc = DateTimeConverter.ToUtcDateTime(closingDateLocal);

        // Obtener detalles
        var yieldDetails = await _yieldDetailRepository
            .GetByPortfolioAndDateAsync(portfolioId, closingDateUtc, ct);

        if (!yieldDetails.Any())
        {
           // throw new BusinessRuleValidationException("No hay detalles de rendimiento para consolidar.");
        }

        // Calcular sumatorias
        var income = yieldDetails.Sum(x => x.Income);
        var expenses = yieldDetails.Sum(x => x.Expenses);
        var commissions = yieldDetails.Sum(x => x.Commissions);
        var costs = expenses + commissions;
        var yieldToCredit = income - costs;
        var processDate = DateTime.UtcNow;

        // Crear entidad de dominio Yield
        var yieldResult = Domain.Yields.Yield.Create(
            portfolioId,
            income,
            expenses,
            commissions,
            costs,
            yieldToCredit,
            closingDateUtc,
            processDate,
            isClosed);

        if (yieldResult.IsFailure)
        {
            //throw new BusinessRuleValidationException(yieldResult.Error);
        }

        // Persistir
        await _yieldRepository.InsertAsync(yieldResult.Value, ct);

        // Calcular valores de simulación solo si NO está cerrado y NO es primer cierre
        SimulationValues simulationValues = new(null, null);

        var isFirstClosing = false;// !await _yieldRepository.ExistsClosedYieldAsync(portfolioId, closingDate, ct);

        if (!isClosed && !isFirstClosing)
        {
            var previousPortfolioValuation = await _portfolioValuationRepository.GetValuationAsync(portfolioId, closingDateUtc.AddDays(-1), ct);
            var previousPortfolioValue = previousPortfolioValuation?.Amount ?? 0;
            var previousUnitValue = previousPortfolioValuation?.UnitValue ?? 0;
            var previousUnits = previousPortfolioValuation?.Units ?? 0;

            simulationValues = SimulationYieldCalculator.Calculate(
                yieldToCredit,
                previousPortfolioValue,
                previousUnitValue,
                previousUnits);
        }

        return new SimulatedYieldResult
        {
            Income = income,
            Expenses = expenses,
            Commissions = commissions,
            Costs = costs,
            YieldToCredit = yieldToCredit,
            UnitValue = simulationValues.UnitValue,
            DailyProfitability = simulationValues.DailyProfitability
        };
    }
}
