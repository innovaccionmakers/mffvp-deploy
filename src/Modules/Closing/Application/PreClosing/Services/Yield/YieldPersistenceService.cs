using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.PreClosing;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Exceptions;
using Common.SharedKernel.Application.Helpers.General;

namespace Closing.Application.PreClosing.Services.Yield;
public sealed class YieldPersistenceService : IYieldPersistenceService
{
    private readonly IYieldDetailRepository _yieldDetailRepository;
    private readonly IYieldRepository _yieldRepository;
    private readonly IPortfolioValuationRepository _portfolioValuationRepository;
    private readonly IConfigurationParameterRepository _configurationParameterRepository;

    public YieldPersistenceService(
        IYieldDetailRepository yieldDetailRepository,
        IYieldRepository yieldRepository,
        IPortfolioValuationRepository portfolioValuationRepository,
        IConfigurationParameterRepository configurationParameterRepository)
    {
        _yieldDetailRepository = yieldDetailRepository;
        _yieldRepository = yieldRepository;
        _portfolioValuationRepository = portfolioValuationRepository;
        _configurationParameterRepository = configurationParameterRepository;
    }

    public async Task<SimulatedYieldResult> ConsolidateAsync(
        RunSimulationParameters parameters,
        CancellationToken ct = default)
    {
        var yieldDetails = await _yieldDetailRepository
            .GetByPortfolioAndDateAsync(parameters.PortfolioId, parameters.ClosingDate, ct);

        if (!yieldDetails.Any())
            throw new BusinessRuleValidationException("No hay detalles de rendimiento para consolidar.");

        var summary = CalculateYieldSummary(yieldDetails);
        var processDate = DateTime.UtcNow;

        var yieldResult = Domain.Yields.Yield.Create(
            parameters.PortfolioId,
            summary.Income,
            summary.Expenses,
            summary.Commissions,
            summary.Costs,
            summary.YieldToCredit,
            parameters.ClosingDate,
            processDate,
            parameters.IsClosing);

        if (yieldResult.IsFailure)
            throw new BusinessRuleValidationException(yieldResult.Error.ToString());

        await _yieldRepository.InsertAsync(yieldResult.Value, ct);

        var simulationValues = await CalculateSimulationValuesAsync(parameters, summary.YieldToCredit, ct);

        return new SimulatedYieldResult
        {
            Income = summary.Income,
            Expenses = summary.Expenses,
            Commissions = summary.Commissions,
            Costs = summary.Costs,
            YieldToCredit = summary.YieldToCredit,
            UnitValue = simulationValues.UnitValue,
            DailyProfitability = simulationValues.DailyProfitability
        };
    }

    private static YieldSummary CalculateYieldSummary(IEnumerable<YieldDetail> details)
    {
        var income = details.Sum(x => x.Income);
        var expenses = details.Sum(x => x.Expenses);
        var commissions = details.Sum(x => x.Commissions);

        return new YieldSummary(income, expenses, commissions);
    }

    private async Task<SimulationValues> CalculateSimulationValuesAsync(
        RunSimulationParameters parameters,
        decimal yieldToCredit,
        CancellationToken ct)
    {
        if (parameters.IsClosing)
            return new SimulationValues(null, null);

        if (parameters.IsFirstClosingDay)
        {
            var param = await _configurationParameterRepository
                .GetByUuidAsync(ConfigurationParameterUuids.Closing.InitialFundUnitValue, ct);

            var initialFundUnitValue = JsonDecimalHelper.ExtractDecimal(param?.Metadata, "Valor");
            return new SimulationValues(initialFundUnitValue, null);
        }

        var previousValuation = await _portfolioValuationRepository
            .GetValuationAsync(parameters.PortfolioId, parameters.ClosingDate.AddDays(-1), ct);

        return SimulationYieldCalculator.Calculate(
            yieldToCredit,
            previousValuation?.Amount ?? 0,
            previousValuation?.UnitValue ?? 0,
            previousValuation?.Units ?? 0);
    }
}