using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.SubtransactionTypes;
using Closing.Domain.ClientOperations;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.Yields;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.SubtransactionTypes;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.PortfolioValuation;

public class PortfolioValuationService(
    IPortfolioValuationRepository valuationRepository,
    IClientOperationRepository clientOperationRepository,
    IYieldRepository yieldRepository,
    ISubtransactionTypesService subtransactionTypes,
    IUnitOfWork unitOfWork,
    ILogger<PortfolioValuationService> logger)
    : IPortfolioValuationService
{
    private const decimal InitialUnitValue = 10000;

    public async Task<Result> CalculateAndPersistValuationAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        // 1. Validar existencia previa de cierre para esa fecha
        var exists = await valuationRepository.ValuationExistsAsync(portfolioId, closingDate.Date, ct);
        if (exists)
        {
            return Result.Failure(new Error("001", "Ya existe una valoración cerrada para este portafolio y fecha.", ErrorType.Validation));
        }

        // 2. Obtener valoración del día anterior
        var previousValuation = await valuationRepository.GetValuationAsync(
            portfolioId,
            closingDate.AddDays(-1),
            ct);

        decimal prevValue = previousValuation?.Amount ?? 0;
        decimal prevUnits = previousValuation?.Units ?? 0;
        decimal prevUnitValue = previousValuation?.UnitValue ?? 0;

        // 3. Obtener rendimientos del día
        var yield = await yieldRepository.GetByPortfolioAndDateAsync(
            portfolioId,
            closingDate,
            ct);

        decimal yieldIncome = yield?.Income ?? 0;
        decimal yieldCosts = yield?.Costs ?? 0;
        decimal yieldToCredit = yield?.YieldToCredit ?? 0;

        // 4. Obtener subtipos de transacción y clasificarlos
       var subtypeResult = await subtransactionTypes.GetAllAsync(ct);
        if (!subtypeResult.IsSuccess)
            return Result.Failure(subtypeResult.Error!);

        var incomeSubtypes = subtypeResult.Value
            .Where(s => s.Nature == IncomeEgressNature.Income)
            .Select(s => s.SubtransactionTypeId)
            .ToList();

        var egressSubtypes = subtypeResult.Value
            .Where(s => s.Nature == IncomeEgressNature.Egress)
            .Select(s => s.SubtransactionTypeId)
            .ToList();

        // 5. Sumar operaciones de entrada y salida
        var incoming = await clientOperationRepository.SumByPortfolioAndSubtypesAsync(
            portfolioId,
            closingDate,
            incomeSubtypes,
            ct);

        var outgoing = await clientOperationRepository.SumByPortfolioAndSubtypesAsync(
            portfolioId,
            closingDate,
            egressSubtypes,
            ct);

        // 6. Si es el primer día (no hay valoración previa)
        if (previousValuation == null)
        {
            if (incoming == 0)
                return Result.Failure(new Error("002", "No se puede calcular la valoración inicial sin operaciones de entrada.", ErrorType.Validation));

            prevUnits = incoming / InitialUnitValue;
            prevUnitValue = InitialUnitValue;
        }

        // 7. Cálculos financieros
        decimal newValue = prevValue + yieldToCredit + incoming - outgoing;
        decimal newUnitValue = prevValue + yieldToCredit == 0 || prevUnits == 0
            ? 0
            : Math.Round((prevValue + yieldToCredit) / prevUnits, 2);
        decimal newUnits = newUnitValue == 0 ? 0 : newValue / newUnitValue;

        decimal grossYieldPerUnit = prevUnits == 0 ? 0 : yieldIncome / prevUnits;
        decimal costPerUnit = prevUnits == 0 ? 0 : yieldCosts / prevUnits;
        decimal dailyProfitability = prevUnitValue == 0 ? 0 : (decimal)Math.Pow((double)(newUnitValue / prevUnitValue), 365) - 1;

        // 8. Crear entidad y guardar
        var result = Domain.PortfolioValuations.PortfolioValuation.Create(
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
            DateTime.UtcNow,
            true);

        if (!result.IsSuccess)
            return Result.Failure(result.Error!);

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("[PortfolioValuation] Valoración realizada para Portafolio {PortfolioId} en {Date}",
            portfolioId, closingDate);

        return Result.Success();
    }
}
