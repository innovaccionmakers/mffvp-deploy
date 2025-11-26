using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Closing.Application.Closing.Services.OperationTypes;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Domain.ClientOperations;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.YieldDetails;
using Closing.Domain.Yields;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Application.Constants;
using Common.SharedKernel.Application.Helpers.Finance;
using Common.SharedKernel.Application.Helpers.Money;
using Common.SharedKernel.Application.Helpers.Serialization;
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
    ILogger<PortfolioValuationService> logger,
    IYieldDetailRepository yieldDetailRepository)
    : IPortfolioValuationService
{
    public async Task<Result<PrepareClosingResult>> CalculateAndPersistValuationAsync(
        int portfolioId,
        DateTime closingDate,
         bool hasDebitNotes,
        CancellationToken cancellationToken)
    {
        using var _ = logger.BeginScope(new Dictionary<string, object>
        {
            ["PortfolioId"] = portfolioId,
            ["ClosingDate"] = closingDate.Date
        });


        var now = DateTime.UtcNow;

        // ⬅️ Se actualiza el paso de cierre usando el flujo estándar
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingPortfolioValuation", now, cancellationToken);

        // 1. Validar existencia previa de cierre para esa fecha
        if (await valuationRepository.ExistsByPortfolioAndDateAsync(portfolioId, closingDate.Date, cancellationToken))
            return Result.Failure<PrepareClosingResult>(
                new Error("001", "Ya existe una valoración cerrada para este portafolio y fecha.", ErrorType.Validation));


        // 2. Obtener valoración del día anterior
        var previousPV = await valuationRepository.GetReadOnlyByPortfolioAndDateAsync(
            portfolioId,
            closingDate.AddDays(-1),
            cancellationToken);

        decimal prevPVAmount =previousPV?.Amount ?? 0m;
        decimal prevPVUnits = previousPV?.Units ?? 0m;
        decimal prevPVUnitValue = previousPV?.UnitValue ?? 0m;


        // 3. Obtener rendimientos del día
        var yield = await yieldRepository.GetReadOnlyByPortfolioAndDateAsync(
            portfolioId,
            closingDate,
            cancellationToken);

        decimal yieldIncome = yield?.Income ?? 0m;
        decimal yieldExpenses = yield?.Expenses ?? 0m;   // para "Egresos"
        decimal yieldCommissions = yield?.Commissions ?? 0m;   // para "Comision"
        decimal yieldToCredit = yield?.YieldToCredit ?? 0m;   // para "RendimientosAbonar"
        decimal yieldCosts = yield?.Costs ?? 0m;   // para "Costos"

        // 4. Obtener y clasificar los tipos de operación

        var operationTypesResult = await operationTypes.GetAllAsync(cancellationToken);

        if (!operationTypesResult.IsSuccess)
            return Result.Failure<PrepareClosingResult>(operationTypesResult.Error!);

        var groupFilter = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            OperationTypeAttributes.GroupList.ClientOperations,
            OperationTypeAttributes.GroupList.AccountingNotes
        };

        var allActiveOperationTypes = operationTypesResult.Value;

        var operationsGroupsClosingInOut = allActiveOperationTypes
        .Where(operationType =>
        {
            var groupList = operationType.GetGroupList();
            return !string.IsNullOrWhiteSpace(groupList)
                   && groupFilter.Contains(groupList);
        })
        .ToList();

        // Ids de los tipos base(padres) que pertenecen a los grupos indicados
        var parentIdsFromGroups = new HashSet<string>(
            operationsGroupsClosingInOut.Select(operationType => operationType.Name)
        );

        // Hijos: tipos cuya Category (padre) está en ese conjunto
        var childrenOfGroupedParents = allActiveOperationTypes
            .Where(operationType =>
                !string.IsNullOrWhiteSpace(operationType.Category) &&
                parentIdsFromGroups.Contains(operationType.Category))
            .ToList();

        // Operaciones Agrupadas (padres e hijos), sin duplicados
        var closingOperations = operationsGroupsClosingInOut
            .Concat(childrenOfGroupedParents)
            .GroupBy(operationType => operationType.OperationTypeId)
            .Select(group => group.First())
            .ToList();

        //Obtener solo los tipos de operacion que se catalogan como operaciones de entrada o salida 

        var incomeOperIds = closingOperations
          .Where(operationType => operationType.Nature == IncomeEgressNature.Income)
          .Select(operationType => operationType.OperationTypeId)
          .ToList();

        var egressOperIds = closingOperations
            .Where(operationType => operationType.Nature == IncomeEgressNature.Egress)
            .Select(operationType => operationType.OperationTypeId)
            .ToList();
      
        // 5. Sumar operaciones de entrada y salida del día
        var incoming =MoneyHelper.Round2(await clientOperationRepository
            .SumByPortfolioAndSubtypesAsync(portfolioId, closingDate, incomeOperIds, new[] { LifecycleStatus.Active }, cancellationToken));
        var outgoing = MoneyHelper.Round2(await clientOperationRepository
            .SumByPortfolioAndSubtypesAsync(portfolioId, closingDate, egressOperIds, new[] { LifecycleStatus.Active }, cancellationToken));

        // Si hay notas de débito, sumar el valor extra por notas de débito 
        var debitNoteExtraReturn = 0m;
        if (hasDebitNotes)
        {
            debitNoteExtraReturn = MoneyHelper.Round2(await yieldDetailRepository.
                GetExtraReturnIncomeSumAsync(
                    portfolioId,
                    closingDate,
                    cancellationToken));
        }

        outgoing += debitNoteExtraReturn;

        var shouldCalculateUnits = incoming != 0 || outgoing != 0;

        // 6. Si es el primer día de cierre, calcular units y unitValue iniciales
        if (previousPV == null)
        {
            if (incoming == 0)
                return Result.Failure<PrepareClosingResult>(
                    new Error("002", "No se puede calcular la valoración inicial sin operaciones de entrada.", ErrorType.Validation));

            var param = await configurationParameterRepository
               .GetByUuidAsync(ConfigurationParameterUuids.Closing.InitialFundUnitValue, cancellationToken);

            var initialUnitValue = JsonDecimalHelper.ExtractDecimal(param?.Metadata, "valor");

            prevPVUnits = incoming / initialUnitValue;
            prevPVUnitValue = initialUnitValue;

        }

        // 7. CÁLCULOS FINANCIEROS

        // 7.1. Nuevo valor del portafolio:
        //     prevValue + rendimientosAbonar + operacionesEntrada - operacionesSalida
        decimal newValue = PortfolioMath.CalculateNewPortfolioValue(
            prevPVAmount,
            yieldToCredit,
            incoming,
            outgoing, 
            DecimalPrecision.TwoDecimals);

        // 7.2. Nuevo valor de unidad:
        //     Si no hay valoración previa, mantener prevUnitValue,
        //     de lo contrario: (prevValue + yieldToCredit) / prevUnits, redondeado a 16 decimales
        decimal newUnitValue = previousPV == null
            ? prevPVUnitValue
            : PortfolioMath.CalculateRoundedUnitValue(
                prevPVAmount,
                yieldToCredit,
                prevPVUnits,
                DecimalPrecision.SixteenDecimals);

        // 7.3. Nuevas unidades del portafolio:
        //     newValue / newUnitValue, redondeado a 16 decimales
        decimal newUnits;
        if (previousPV is null || !shouldCalculateUnits)
        {
            newUnits = prevPVUnits;
        }
        else
        {
            newUnits = PortfolioMath.CalculateNewUnits(
                 newValue,
                 newUnitValue,
                 DecimalPrecision.SixteenDecimals);
        }

        // 7.4. Rendimiento bruto por unidad:
        //     yieldIncome / prevUnits, redondeado a 16 decimales
        decimal grossYieldPerUnit = previousPV == null
            ? 0m
            : PortfolioMath.CalculateGrossYieldPerUnitFromIncome(
                yieldIncome,
                prevPVUnits,
                DecimalPrecision.SixteenDecimals);

        // 7.5. Costo por unidad:
        //     yieldCosts / prevUnits, redondeado a 16 decimales
        decimal costPerUnit = previousPV == null
            ? 0m
            : PortfolioMath.CalculateCostPerUnit(
                yieldCosts,
                prevPVUnits,
                DecimalPrecision.SixteenDecimals);

        // 7.6. Rentabilidad diaria:
        //     (newUnitValue / prevUnitValue)- 1, redondeado a 16 decimales
        decimal dailyProfitability = previousPV == null
            ? 0m
            : PortfolioMath.CalculateRoundedDailyProfitability(
                prevPVUnitValue,
                newUnitValue,
                DecimalPrecision.SixteenDecimals
                );

        // 8. Crear entidad de valoración y persistir
        var createResult = Domain.PortfolioValuations.PortfolioValuation.Create(
            portfolioId,
            closingDate,
            newValue,
            newValue,
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
            return Result.Failure<PrepareClosingResult>(createResult.Error!);

        await valuationRepository.AddAsync(createResult.Value);

        // 9. Construir y devolver PrepareClosingResult con todos los datos financieros
        var closedResult = new PrepareClosingResult(portfolioId, closingDate)
        {
            Income = yieldIncome,
            Expenses = yieldExpenses,
            Commissions = yieldCommissions,
            Costs = yieldCosts,
            YieldToCredit = yieldToCredit,
            UnitValue =MoneyHelper.Round2(newUnitValue),
            DailyProfitability = MoneyHelper.RoundToScale(dailyProfitability * 100, DecimalPrecision.SixDecimals)
        };

        return Result.Success(closedResult);
    }
}
