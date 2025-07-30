using Closing.Application.Abstractions.Data;
using Closing.Application.Closing.Services.Orchestation.Constants;
using Closing.Application.Closing.Services.SubtransactionTypes;
using Closing.Domain.ClientOperations;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.Yields;
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
    IUnitOfWork unitOfWork,
    ILogger<PortfolioValuationService> logger)
    : IPortfolioValuationService
{
   

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

            var param = await configurationParameterRepository
               .GetByUuidAsync(ConfigurationParameterUuids.Closing.InitialFundUnitValue, ct);

            var InitialUnitValue = JsonDecimalHelper.ExtractDecimal(param?.Metadata, "Valor");

            prevUnits = incoming / InitialUnitValue;
            prevUnitValue = InitialUnitValue;
        }
        // 7. Cálculos financieros
        //nuevo valor del portafolio, dado por Valor del portafolio del día anterior +
        //rendimientos a abonar tomados de la tabla rendimientos para la fecha +
        //Valoración_portafolio.operaciones_entrada -
        //Valoración_portafolio.operaciones_salida
        decimal newValue = PortfolioMath.CalculateNewPortfolioValue(prevValue, yieldToCredit, incoming, outgoing);
        //guarda el valor de la unidad para la fecha en cuestión,
        //la cual está dada por el valor del portafolio del día anterior (Valoracion_portafolio.valor)
        //+ rendimientos a abonar para el día (Rendimiento.rendimientos_abonar)
        //dividido el número de unidades del día anterior (Valoracion_portafolio.unidades).
        //Math.Round((prevValue + yieldToCredit) / prevUnits, DecimalPrecision.SixteenDecimals);
        decimal newUnitValue = prevValue + yieldToCredit == 0 || prevUnits == 0
            ? 0
            : PortfolioMath.CalculateRoundedUnitValue(prevValue, yieldToCredit, prevUnits, DecimalPrecision.SixteenDecimals);
        //almacena el número de unidades del portafolio,
        //estas se calculan tomando el nuevo valor del fondo para el día (Valoracion_portafolio.valor)
        //dividido por el nuevo valor de unidad (Valoracion_portafolio.valor_unidad).
        decimal newUnits = newUnitValue == 0 ? 0 : PortfolioMath.CalculateNewUnits(newValue , newUnitValue, DecimalPrecision.SixteenDecimals);

        // equivale al rendimiento bruto del día en cuestión (Rendimiento.ingresos)
        // dividido el número de unidades del día anterior (Valoracion_portafolio.unidades).
        decimal grossYieldPerUnit = prevUnits == 0 ? 0 : PortfolioMath.CalculateGrossYieldPerUnitFromIncome(yieldIncome, prevUnits, DecimalPrecision.SixteenDecimals);
        //equivale al costo del día (Rendimiento.costos)
        //dividido el número de unidades del día anterior (Valoracion_portafolio.unidades).
        decimal costPerUnit = prevUnits == 0 ? 0 : PortfolioMath.CalculateCostPerUnit(yieldCosts, prevUnits, DecimalPrecision.SixteenDecimals);
        // Calcula la rentabilidad diaria, que es el crecimiento del valor de la unidad
        //fórmula (valor_unidad día/valor_unidad día anterior)^(365)-1. Considere el valor de unidad desde el campo Valoracion_portafolio.valor_unidad.
        decimal dailyProfitability = prevUnitValue == 0 ? 0 : PortfolioMath.CalculateRoundedDailyProfitability( prevUnitValue, newUnitValue, DecimalPrecision.SixteenDecimals, 365);
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

        logger.LogInformation("Valoración realizada para Portafolio {PortfolioId} en {Date}",
            portfolioId, closingDate);

        return Result.Success();
    }
}
