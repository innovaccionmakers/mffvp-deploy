
using Closing.Application.Closing.Services.Orchestation.Constants;
using Closing.Application.Closing.Services.SubtransactionTypes;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Domain.ClientOperations;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.Yields;
using Closing.Integrations.Closing.RunClosing;    
using Common.SharedKernel.Application.Helpers.Finance;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.SubtransactionTypes;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.PortfolioValuation;

public class PortfolioValuationService(
    IPortfolioValuationRepository valuationRepository,
    IClientOperationRepository clientOperationRepository,
    IYieldRepository yieldRepository,
    ISubtransactionTypesService subtransactionTypes,
    IConfigurationParameterRepository configurationParameterRepository,
    ITimeControlService timeControlService,
    //IUnitOfWork unitOfWork,
    ILogger<PortfolioValuationService> logger)
    : IPortfolioValuationService
{
    public async Task<Result<ClosedResult>> CalculateAndPersistValuationAsync(
        int portfolioId,
        DateTime closingDate,
        CancellationToken ct)
    {
        logger.LogInformation("Iniciando valoración para portafolio {PortfolioId}", portfolioId);

        var now = DateTime.UtcNow;

        // ⬅️ Se actualiza el paso de cierre usando el flujo estándar
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingPortfolioValuation", now, ct);
        // 1. Validar existencia previa de cierre para esa fecha
        if (await valuationRepository.ValuationExistsAsync(portfolioId, closingDate.Date, ct))
            return Result.Failure<ClosedResult>(
                new Error("001", "Ya existe una valoración cerrada para este portafolio y fecha.", ErrorType.Validation));

        // 2. Obtener valoración del día anterior
        var previous = await valuationRepository.GetValuationAsync(
            portfolioId,
            closingDate.AddDays(-1),
            ct);

        decimal prevValue = previous?.Amount ?? 0m;
        decimal prevUnits = previous?.Units ?? 0m;
        decimal prevUnitValue = previous?.UnitValue ?? 0m;

        // 3. Obtener rendimientos del día
        var yield = await yieldRepository.GetByPortfolioAndDateAsync(
            portfolioId,
            closingDate,
            ct);

        decimal yieldIncome = yield?.Income ?? 0m;
        decimal yieldExpenses = yield?.Expenses ?? 0m;   // para "Egresos"
        decimal yieldCommissions = yield?.Commissions ?? 0m;   // para "Comision"
        decimal yieldToCredit = yield?.YieldToCredit ?? 0m;   // para "RendimientosAbonar"
        decimal yieldCosts = yield?.Costs ?? 0m;   // para "Costos"

        // 4. Obtener y clasificar subtipos de transacción
        var subtypeResult = await subtransactionTypes.GetAllAsync(ct);
        if (!subtypeResult.IsSuccess)
            return Result.Failure<ClosedResult>(subtypeResult.Error!);

        var incomeSubs = subtypeResult.Value
            .Where(s => s.Nature == IncomeEgressNature.Income)
            .Select(s => s.SubtransactionTypeId)
            .ToList();
        var egressSubs = subtypeResult.Value
            .Where(s => s.Nature == IncomeEgressNature.Egress)
            .Select(s => s.SubtransactionTypeId)
            .ToList();

        // 5. Sumar operaciones de entrada y salida del día
        var incoming = await clientOperationRepository
            .SumByPortfolioAndSubtypesAsync(portfolioId, closingDate, incomeSubs, ct);
        var outgoing = await clientOperationRepository
            .SumByPortfolioAndSubtypesAsync(portfolioId, closingDate, egressSubs, ct);

        // 6. Si es el primer día de cierre, calcular units y unitValue iniciales
        if (previous == null)
        {
            if (incoming == 0)
                return Result.Failure<ClosedResult>(
                    new Error("002", "No se puede calcular la valoración inicial sin operaciones de entrada.", ErrorType.Validation));

            var param = await configurationParameterRepository
               .GetByUuidAsync(ConfigurationParameterUuids.Closing.InitialFundUnitValue, ct);

            var initialUnitValue = JsonDecimalHelper.ExtractDecimal(param?.Metadata, "Valor");

            prevUnits = incoming / initialUnitValue;
            prevUnitValue = initialUnitValue;
        }

        // 7. CÁLCULOS FINANCIEROS

        // 7.1. Nuevo valor del portafolio:
        //     prevValue + rendimientosAbonar + operacionesEntrada - operacionesSalida
        decimal newValue = PortfolioMath.CalculateNewPortfolioValue(
            prevValue,
            yieldToCredit,
            incoming,
            outgoing);

        // 7.2. Nuevo valor de unidad:
        //     Si no hay valoración previa, mantener prevUnitValue,
        //     de lo contrario: (prevValue + yieldToCredit) / prevUnits, redondeado a 16 decimales
        decimal newUnitValue = previous == null
            ? prevUnitValue
            : PortfolioMath.CalculateRoundedUnitValue(
                prevValue,
                yieldToCredit,
                prevUnits,
                DecimalPrecision.SixteenDecimals);

        // 7.3. Nuevas unidades del portafolio:
        //     newValue / newUnitValue, redondeado a 16 decimales
        decimal newUnits = previous == null
            ? prevUnits
            : PortfolioMath.CalculateNewUnits(
                newValue,
                newUnitValue,
                DecimalPrecision.SixteenDecimals);

        // 7.4. Rendimiento bruto por unidad:
        //     yieldIncome / prevUnits, redondeado a 16 decimales
        decimal grossYieldPerUnit = previous == null
            ? 0m
            : PortfolioMath.CalculateGrossYieldPerUnitFromIncome(
                yieldIncome,
                prevUnits,
                DecimalPrecision.SixteenDecimals);

        // 7.5. Costo por unidad:
        //     yieldCosts / prevUnits, redondeado a 16 decimales
        decimal costPerUnit = previous == null
            ? 0m
            : PortfolioMath.CalculateCostPerUnit(
                yieldCosts,
                prevUnits,
                DecimalPrecision.SixteenDecimals);

        // 7.6. Rentabilidad diaria:
        //     (newUnitValue / prevUnitValue)^(365) - 1, redondeado a 16 decimales
        decimal dailyProfitability = previous == null
            ? 0m
            : PortfolioMath.CalculateRoundedDailyProfitability(
                prevUnitValue,
                newUnitValue,
                DecimalPrecision.SixteenDecimals,
                365);

        // 8. Crear entidad de valoración y persistir
        var createResult = Domain.PortfolioValuations.PortfolioValuation.Create(
            portfolioId,
            closingDate,
            Math.Round(newValue, 2),
            newUnits,
            newUnitValue,
            grossYieldPerUnit,
            costPerUnit,
            dailyProfitability,
            incoming,
            outgoing,
            now,
            true);

        if (!createResult.IsSuccess)
            return Result.Failure<ClosedResult>(createResult.Error!);

        await valuationRepository.AddAsync(createResult.Value);

        logger.LogInformation(
            "Valoración realizada para Portafolio {PortfolioId} en {Date}",
            portfolioId,
            closingDate);

        // 9. Construir y devolver ClosedResult con todos los datos financieros
        var closedResult = new ClosedResult(portfolioId, closingDate)
        {
            Income = yieldIncome,
            Expenses = yieldExpenses,
            Commissions = yieldCommissions,
            Costs = yieldCosts,
            YieldToCredit = yieldToCredit,
            UnitValue = newUnitValue,
            DailyProfitability = dailyProfitability
        };

        return Result.Success(closedResult);
    }
}
