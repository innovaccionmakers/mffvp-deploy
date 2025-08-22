using Closing.Application.Closing.Services.OperationTypes;
using Closing.Application.Closing.Services.Orchestation.Constants;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Domain.ClientOperations;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.Yields;
using Closing.Integrations.Closing.RunClosing;

using Common.SharedKernel.Application.Helpers.Finance;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.OperationTypes;

using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.PortfolioValuation;

public class PortfolioValuationService(
    IPortfolioValuationRepository valuationRepository,
    IClientOperationRepository clientOperationRepository,
    IYieldRepository yieldRepository,
    IOperationTypesService operationTypes,
    IConfigurationParameterRepository configurationParameterRepository,
    ITimeControlService timeControlService,
    //IUnitOfWork unitOfWork,
    ILogger<PortfolioValuationService> logger)
    : IPortfolioValuationService
{
    public async Task<Result<ClosedResult>> CalculateAndPersistValuationAsync(
        int portfolioId,
        DateTime closingDate,
        CancellationToken cancellationToken)
    {
        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            ["PortfolioId"] = portfolioId,
            ["ClosingDate"] = closingDate.Date
        });

        logger.LogInformation("Iniciando valoración para portafolio {PortfolioId}", portfolioId);

        var now = DateTime.UtcNow;

        // ⬅️ Se actualiza el paso de cierre usando el flujo estándar
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingPortfolioValuation", now, cancellationToken);
        logger.LogInformation("Paso de control de tiempo actualizado: NowUtc={NowUtc}", now);

        // 1. Validar existencia previa de cierre para esa fecha
        logger.LogInformation("Verificando si existe valoración previa cerrada para la fecha {ClosingDate}", closingDate.Date);
        if (await valuationRepository.ExistsByPortfolioAndDateAsync(portfolioId, closingDate.Date, cancellationToken))
            return Result.Failure<ClosedResult>(
                new Error("001", "Ya existe una valoración cerrada para este portafolio y fecha.", ErrorType.Validation));
        logger.LogInformation("No existe valoración cerrada previa para la fecha {ClosingDate}", closingDate.Date);

        // 2. Obtener valoración del día anterior
        var previous = await valuationRepository.GetReadOnlyByPortfolioAndDateAsync(
            portfolioId,
            closingDate.AddDays(-1),
            cancellationToken);

        decimal prevValue = Math.Round(previous?.Amount ?? 0m, DecimalPrecision.TwoDecimals);
        decimal prevUnits = Math.Round(previous?.Units ?? 0m, DecimalPrecision.SixteenDecimals);
        decimal prevUnitValue = Math.Round(previous?.UnitValue ?? 0m, DecimalPrecision.TwoDecimals);
        logger.LogInformation("Valoración previa: Amount={PrevValue}, Units={PrevUnits}, UnitValue={PrevUnitValue}",
            prevValue, prevUnits, prevUnitValue);

        // 3. Obtener rendimientos del día
        var yield = await yieldRepository.GetByPortfolioAndDateAsync(
            portfolioId,
            closingDate,
            cancellationToken);

        logger.LogInformation("Rendimientos dia Portafolio: " + yield);

        decimal yieldIncome = Math.Round(yield?.Income ?? 0m, DecimalPrecision.TwoDecimals);
        decimal yieldExpenses = Math.Round(yield?.Expenses ?? 0m, DecimalPrecision.TwoDecimals);   // para "Egresos"
        decimal yieldCommissions = Math.Round(yield?.Commissions ?? 0m, DecimalPrecision.TwoDecimals);   // para "Comision"
        decimal yieldToCredit = Math.Round(yield?.YieldToCredit ?? 0m, DecimalPrecision.TwoDecimals);   // para "RendimientosAbonar"
        decimal yieldCosts = Math.Round(yield?.Costs ?? 0m, DecimalPrecision.TwoDecimals);   // para "Costos"
        logger.LogInformation("Rendimientos del día: Income={Income}, Expenses={Expenses}, Commissions={Commissions}, YieldToCredit={YieldToCredit}, Costs={Costs}",
            yieldIncome, yieldExpenses, yieldCommissions, yieldToCredit, yieldCosts);

        // 4. Obtener y clasificar subtipos de transacción
        var subtypeResult = await operationTypes.GetAllAsync(cancellationToken);
        if (!subtypeResult.IsSuccess)
            return Result.Failure<ClosedResult>(subtypeResult.Error!);

        var incomeSubs = subtypeResult.Value
            .Where(s => s.Nature == IncomeEgressNature.Income && !string.IsNullOrWhiteSpace(s.Category))
            .Select(s => s.OperationTypeId)
            .ToList();
        var egressSubs = subtypeResult.Value
            .Where(s => s.Nature == IncomeEgressNature.Egress && !string.IsNullOrWhiteSpace(s.Category))
            .Select(s => s.OperationTypeId)
            .ToList();
        logger.LogInformation("Subtipos: IncomeCount={IncomeCount}, EgressCount={EgressCount}", incomeSubs.Count, egressSubs.Count);

        // 5. Sumar operaciones de entrada y salida del día
        var incoming = Math.Round(await clientOperationRepository
            .SumByPortfolioAndSubtypesAsync(portfolioId, closingDate, incomeSubs, cancellationToken), DecimalPrecision.TwoDecimals);
        var outgoing = Math.Round(await clientOperationRepository
            .SumByPortfolioAndSubtypesAsync(portfolioId, closingDate, egressSubs, cancellationToken), DecimalPrecision.TwoDecimals);
        logger.LogInformation("Suma de operaciones: Incoming={Incoming}, Outgoing={Outgoing}", incoming, outgoing);

        // 6. Si es el primer día de cierre, calcular units y unitValue iniciales
        if (previous == null)
        {
            if (incoming == 0)
                return Result.Failure<ClosedResult>(
                    new Error("002", "No se puede calcular la valoración inicial sin operaciones de entrada.", ErrorType.Validation));

            var param = await configurationParameterRepository
               .GetByUuidAsync(ConfigurationParameterUuids.Closing.InitialFundUnitValue, cancellationToken);

            var initialUnitValue = JsonDecimalHelper.ExtractDecimal(param?.Metadata, "valor");

            prevUnits = incoming / initialUnitValue;
            prevUnitValue = initialUnitValue;

            logger.LogInformation("Inicialización (primer día): InitialUnitValue={InitialUnitValue}, PrevUnits={PrevUnits}, PrevUnitValue={PrevUnitValue}",
                initialUnitValue, prevUnits, prevUnitValue);
        }

        // 7. CÁLCULOS FINANCIEROS

        // 7.1. Nuevo valor del portafolio:
        //     prevValue + rendimientosAbonar + operacionesEntrada - operacionesSalida
        decimal newValue = PortfolioMath.CalculateNewPortfolioValue(
            prevValue,
            yieldToCredit,
            incoming,
            outgoing, 
            DecimalPrecision.TwoDecimals);
        logger.LogInformation("Cálculo newValue = prevValue({PrevValue}) + yieldToCredit({YieldToCredit}) + incoming({Incoming}) - outgoing({Outgoing}) = {NewValue}",
            prevValue, yieldToCredit, incoming, outgoing, newValue);

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
        logger.LogInformation("Cálculo newUnitValue: {NewUnitValue}", newUnitValue);

        // 7.3. Nuevas unidades del portafolio:
        //     newValue / newUnitValue, redondeado a 16 decimales
        decimal newUnits = previous == null
            ? prevUnits
            : PortfolioMath.CalculateNewUnits(
                newValue,
                newUnitValue,
                DecimalPrecision.SixteenDecimals);
        logger.LogInformation("Cálculo newUnits = newValue({NewValue}) / newUnitValue({NewUnitValue}) = {NewUnits}",
            newValue, newUnitValue, newUnits);

        // 7.4. Rendimiento bruto por unidad:
        //     yieldIncome / prevUnits, redondeado a 16 decimales
        decimal grossYieldPerUnit = previous == null
            ? 0m
            : PortfolioMath.CalculateGrossYieldPerUnitFromIncome(
                yieldIncome,
                prevUnits,
                DecimalPrecision.SixteenDecimals);
        logger.LogInformation("Cálculo grossYieldPerUnit = yieldIncome({YieldIncome}) / prevUnits({PrevUnits}) = {GrossYieldPerUnit}",
            yieldIncome, prevUnits, grossYieldPerUnit);

        // 7.5. Costo por unidad:
        //     yieldCosts / prevUnits, redondeado a 16 decimales
        decimal costPerUnit = previous == null
            ? 0m
            : PortfolioMath.CalculateCostPerUnit(
                yieldCosts,
                prevUnits,
                DecimalPrecision.SixteenDecimals);
        logger.LogInformation("Cálculo costPerUnit = yieldCosts({YieldCosts}) / prevUnits({PrevUnits}) = {CostPerUnit}",
            yieldCosts, prevUnits, costPerUnit);

        // 7.6. Rentabilidad diaria:
        //     (newUnitValue / prevUnitValue)^(365) - 1, redondeado a 16 decimales
        decimal dailyProfitability = previous == null
            ? 0m
            : PortfolioMath.CalculateRoundedDailyProfitability(
                prevUnitValue,
                newUnitValue,
                DecimalPrecision.SixteenDecimals,
                365);
        logger.LogInformation("Cálculo dailyProfitability(prevUnitValue={PrevUnitValue}, newUnitValue={NewUnitValue}) = {DailyProfitability}",
            prevUnitValue, newUnitValue, dailyProfitability);

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

        await valuationRepository.InsertAsync(createResult.Value);

        logger.LogInformation(
            "Valoración realizada para Portafolio {PortfolioId} en {Date}",
            portfolioId,
            closingDate);
        logger.LogInformation("Persistido PortfolioValuation: Amount={Amount}, Units={Units}, UnitValue={UnitValue}, Incoming={Incoming}, Outgoing={Outgoing}",
            createResult.Value.Amount, createResult.Value.Units, createResult.Value.UnitValue, incoming, outgoing);

        // 9. Construir y devolver ClosedResult con todos los datos financieros
        var closedResult = new ClosedResult(portfolioId, closingDate)
        {
            Income = yieldIncome,
            Expenses = yieldExpenses,
            Commissions = yieldCommissions,
            Costs = yieldCosts,
            YieldToCredit = yieldToCredit,
            UnitValue = Math.Round(newUnitValue, DecimalPrecision.TwoDecimals),
            DailyProfitability = Math.Round(dailyProfitability * 100, DecimalPrecision.SixDecimals)
        };

        logger.LogInformation("ClosedResult: {@ClosedResult}", closedResult);

        return Result.Success(closedResult);
    }
}
