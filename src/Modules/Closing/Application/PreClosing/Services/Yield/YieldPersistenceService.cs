using Closing.Application.Closing.Services.Warnings;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Dto;
using Closing.Application.PreClosing.Services.Yield.Helpers;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Constants;
using Common.SharedKernel.Application.Exceptions;
using Common.SharedKernel.Application.Helpers.Serialization;

namespace Closing.Application.PreClosing.Services.Yield;
public sealed class YieldPersistenceService : IYieldPersistenceService
{
    private readonly IYieldDetailRepository _yieldDetailRepository;
    private readonly IYieldRepository _yieldRepository;
    private readonly IPortfolioValuationRepository _portfolioValuationRepository;
    private readonly IConfigurationParameterRepository _configurationParameterRepository;
    private readonly IWarningCollector _warningCollector;

    public YieldPersistenceService(
        IYieldDetailRepository yieldDetailRepository,
        IYieldRepository yieldRepository,
        IPortfolioValuationRepository portfolioValuationRepository,
        IConfigurationParameterRepository configurationParameterRepository,
        IWarningCollector warningCollector)
    {
        _yieldDetailRepository = yieldDetailRepository;
        _yieldRepository = yieldRepository;
        _portfolioValuationRepository = portfolioValuationRepository;
        _configurationParameterRepository = configurationParameterRepository;
        _warningCollector = warningCollector;
    }

    public async Task<SimulatedYieldResult> ConsolidateAsync(
        RunSimulationParameters parameters,
        CancellationToken cancellationToken = default)
    {
        var yieldDetails = await _yieldDetailRepository
            .GetReadOnlyByPortfolioAndDateAsync(parameters.PortfolioId, parameters.ClosingDate, parameters.IsClosing, cancellationToken);

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
            0m,
            parameters.ClosingDate,
            processDate,
            parameters.IsClosing);

        if (yieldResult.IsFailure)
            throw new BusinessRuleValidationException(yieldResult.Error.ToString());

        await _yieldRepository.InsertAsync(yieldResult.Value, cancellationToken);

        var simulationValues = await CalculateSimulationValuesAsync(parameters, summary.YieldToCredit, summary.Income, cancellationToken);

        return new SimulatedYieldResult
        {
            Income = Math.Round(summary.Income,DecimalPrecision.TwoDecimals),
            Expenses = Math.Round(summary.Expenses, DecimalPrecision.TwoDecimals),
            Commissions = Math.Round(summary.Commissions, DecimalPrecision.TwoDecimals),
            Costs = Math.Round(summary.Costs,2),
            YieldToCredit = Math.Round(summary.YieldToCredit, DecimalPrecision.TwoDecimals),
            UnitValue = simulationValues.UnitValue != null
           ? Math.Round(simulationValues.UnitValue.Value, DecimalPrecision.TwoDecimals)
           : (decimal?)null,
            DailyProfitability = simulationValues.DailyProfitability != null
           ? Math.Round(simulationValues.DailyProfitability.Value * 100, DecimalPrecision.SixDecimals)
           : (decimal?)null
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
        decimal dailyIncome,
        CancellationToken cancellationToken)
    {
        if (parameters.IsClosing)
            return new SimulationValues(null, null);

        if (parameters.IsFirstClosingDay)
        {
            var param = await _configurationParameterRepository
                .GetByUuidAsync(ConfigurationParameterUuids.Closing.InitialFundUnitValue, cancellationToken);

            var initialFundUnitValue = JsonDecimalHelper.ExtractDecimal(param?.Metadata, "valor");
            return new SimulationValues(initialFundUnitValue, null);
        }

        // Valoración del día anterior (base para validar y calcular)
        var previousValuation = await _portfolioValuationRepository
            .GetReadOnlyByPortfolioAndDateAsync(parameters.PortfolioId, parameters.ClosingDate.AddDays(-1), cancellationToken);

        // Si no hay valoración previa válida, advertir y no calcular (evita overflow/divisiones inválidas)
        if (previousValuation is null || previousValuation.Amount <= 0m || previousValuation.UnitValue <= 0m || previousValuation.Units <= 0m)
        {
            _warningCollector.Add(WarningCatalog.Val006MissingPreviousValuation());
            return new SimulationValues(null, null);
        }

        // Factor configurable (por ej. param 'PreclosingIncomeLimitFactor' en %); fallback 20%
        var maxIncomeFactor = YieldMathLimits.OverflowSafeYieldFraction;
        var preliminaryIncomeLimit = previousValuation.Amount * maxIncomeFactor;

        // Si el ingreso del día es desproporcionado vs la valoración previa, advertir y no calcular (evita error por overflow)
        if (dailyIncome > preliminaryIncomeLimit)
        {
            _warningCollector.Add(
                WarningCatalog.Val004IncomeHighVsPreviousValuation(
                    dailyIncome,
                    previousValuation.Amount,
                    preliminaryIncomeLimit));

            return new SimulationValues(null, null);
        }

        // Cálculo normal si pasa validaciones

        return SimulationYieldCalculator.Calculate(
            yieldToCredit,
            previousValuation?.Amount ?? 0,
            previousValuation?.UnitValue ?? 0,
            previousValuation?.Units ?? 0);
    }
}