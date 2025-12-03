
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustYieldsDistribution.Interfaces;
using Closing.Application.Closing.Services.Warnings;
using Closing.Application.PreClosing.Services.AutomaticConcepts.Dto;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Dto;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Closing.Domain.YieldsToDistribute;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.Money;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using System.Text.Json;

namespace Closing.Application.Closing.Services.TrustYieldsDistribution;

public class ValidateTrustYieldsDistributionService(
    IYieldRepository yieldRepository,
    ITrustYieldRepository trustYieldRepository,
    IYieldToDistributeRepository yieldToDistributeRepository,
    IYieldDetailCreationService yieldDetailCreationService,
    YieldDetailBuilderService yieldDetailBuilderService,
    ITimeControlService timeControlService,
    IConfigurationParameterRepository configurationParameterRepository,
    IWarningCollector warnings,
    IPortfolioValuationRepository portfolioValuationRepository
    )
    : IValidateTrustYieldsDistributionService
{
    public async Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {

        var now = DateTime.UtcNow;

        // Publicar evento de paso
        await timeControlService.UpdateStepAsync(portfolioId, "ClosingAllocationCheck", now, cancellationToken);

        var yieldToCredit = await yieldRepository.GetYieldToCreditAsync(portfolioId, closingDate, cancellationToken);
        if (yieldToCredit is null)
        {
            return Result.Failure(new Error("001", "No se encontró información de rendimientos para la fecha de cierre.", ErrorType.Failure));
        }

        var expectedTotal = MoneyHelper.Round2(yieldToCredit.Value);

        // Obtener el parámetro de configuración del concepto para filtrar rendimientos_por_distribuir
        var adjustmentConceptParam = await configurationParameterRepository.GetByUuidAsync(
            ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, 
            cancellationToken);
        
        string? conceptJson = null;
        if (adjustmentConceptParam?.Metadata != null)
        {
            // Transformar el formato del metadata {"id": 3, "nombre": "..."} a {"EntityId": "3", "EntityValue": "..."}
            var conceptId = JsonIntegerHelper.ExtractInt32(adjustmentConceptParam.Metadata, "id", defaultValue: 0);
            var conceptName = JsonStringHelper.ExtractString(adjustmentConceptParam.Metadata, "nombre", defaultValue: string.Empty);
            
            if (conceptId > 0 && !string.IsNullOrWhiteSpace(conceptName))
            {
                var conceptDto = new StringEntityDto(conceptId.ToString(), conceptName);
                conceptJson = JsonSerializer.Serialize(conceptDto);
            }
        }
        
        // Obtener suma de rendimientos por distribuir filtrados por concepto
        var pendingToDistribute = await yieldToDistributeRepository.GetTotalYieldAmountRoundedAsync(
            portfolioId, 
            closingDate, 
            conceptJson, 
            cancellationToken);
        
        // Calcular el total distribuido (suma de rendimientos_fideicomisos)
        var distributedTotal = await trustYieldRepository.GetDistributedTotalRoundedAsync(portfolioId, closingDate, cancellationToken);

        // Calcular diferencia: rendimientos_abonar - (rendimientos_abonados + rendimientos_por_distribuir)
        var totalAlreadyProcessed = MoneyHelper.Round2(distributedTotal + pendingToDistribute);
        var difference = MoneyHelper.Round2(expectedTotal - totalAlreadyProcessed);
        
        var differenceOrigin = MoneyHelper.Round2(expectedTotal - distributedTotal);

        await yieldRepository.UpdateCreditedYieldsAsync(portfolioId, closingDate, distributedTotal, now, cancellationToken);

        //Diferencia > 0 → faltó distribuir(sobra en portafolio) → Ingreso automático para el día siguiente.
        //Diferencia < 0 → se distribuyó de más(faltante en portafolio) → Gasto automático para el día siguiente  - se almacena en columna Ingresos.

        if (difference != 0m)
        {
            //Obtener parámetros de configuración necesarios
            var uuids = new[]
            {
                ConfigurationParameterUuids.Closing.YieldDifferenceTolerance,
                ConfigurationParameterUuids.Closing.YieldAdjustmentIncome,
                ConfigurationParameterUuids.Closing.YieldAdjustmentExpense
            };

            var map = await configurationParameterRepository.GetReadOnlyByUuidsAsync(uuids, cancellationToken);

            // Tolerancia
            if (!map.TryGetValue(ConfigurationParameterUuids.Closing.YieldDifferenceTolerance, out var toleranceParam)
                || toleranceParam.Metadata is null)
                return Result.Failure(Error.Failure("001", "No se encontró metadata para 'ToleranciaRendimientos'."));

            var tolerance = JsonDecimalHelper.ExtractDecimal(toleranceParam.Metadata, "valor", isPercentage: false);
            if (tolerance < 0m)
                return Result.Failure(Error.Failure("001A", "Tolerancia inválida (< 0)."));

            if (Math.Abs(difference) > tolerance)
            {
                warnings.Add(WarningCatalog.Adv003YieldDifference(difference, tolerance));
            }

            var nextClosingDate = closingDate.AddDays(1);
            var isIncome = difference > 0m;

            // Concepto Automatico según signo
            var conceptUuid = difference > 0m
                ? ConfigurationParameterUuids.Closing.YieldAdjustmentIncome
                : ConfigurationParameterUuids.Closing.YieldAdjustmentExpense;

            if (!map.TryGetValue(conceptUuid, out var adjustmentParam) || adjustmentParam.Metadata is null)
                return Result.Failure(Error.Failure("001", $"No se encontró metadata para '{conceptUuid}'."));

            var conceptId = JsonIntegerHelper.ExtractInt32(adjustmentParam.Metadata, "id", defaultValue: 0);
            var conceptName = JsonStringHelper.ExtractString(adjustmentParam.Metadata, "nombre", defaultValue: string.Empty);

            if (conceptId <= 0 || string.IsNullOrWhiteSpace(conceptName))
                return Result.Failure(new Error("002", $"Metadata inválida para '{conceptUuid}': id/nombre requeridos.", ErrorType.Failure));

            var summary = new AutomaticConceptSummary(
                ConceptId: conceptId,
                ConceptName: conceptName,
                Nature: isIncome ? IncomeExpenseNature.Income : IncomeExpenseNature.Expense,
                Source: YieldsSources.AutomaticConcept,
                TotalAmount: difference
            );

            var parameters = new RunSimulationParameters(
                PortfolioId: portfolioId,
                ClosingDate: nextClosingDate,
                IsClosing: true
            );

            var buildResult = yieldDetailBuilderService.Build(
                new[] { summary }, parameters);

            await yieldDetailCreationService.CreateYieldDetailsAsync(buildResult, PersistenceMode.Transactional, cancellationToken);

            //Ajuste al valor del portafolio con los rendimientos abonados
            await portfolioValuationRepository.ApplyAllocationCheckDiffAsync(portfolioId, closingDate, -differenceOrigin, cancellationToken);
        }
       
        return Result.Success();
    }
}
