using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Closing.Application.Abstractions.External.Operations.OperationTypes;
using Closing.Application.Closing.Services.DistributableReturns.Interfaces;
using Closing.Application.Closing.Services.OperationTypes;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.PreClosing.Services.Yield.Dto;
using Closing.Domain.ClientOperations;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.PortfolioValuations;
using Closing.Domain.TrustYields;
using Closing.Domain.Yields;
using Closing.Domain.YieldsToDistribute;
using Common.SharedKernel.Application.Constants;
using Common.SharedKernel.Application.Helpers.Finance;
using Common.SharedKernel.Application.Helpers.Money;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.DistributableReturns;

public class DistributableReturnsService(
    ITrustYieldRepository trustYieldRepository,
    IClientOperationRepository clientOperationRepository,
    IYieldRepository yieldRepository,
    IPortfolioValuationRepository portfolioValuationRepository,
    IConfigurationParameterRepository configurationParameterRepository,
    IYieldToDistributeRepository yieldToDistributeRepository,
    ITimeControlService timeControlService,
    IOperationTypesService operationTypesService,
    ILogger<DistributableReturnsService> logger)
    : IDistributableReturnsService
{
    private const string DebitNoteOperationName = "Nota Débito";

    private static readonly CompareInfo DebitNoteCompareInfo = CultureInfo.InvariantCulture.CompareInfo;

    public async Task<Result> RunAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["Service"] = nameof(DistributableReturnsService),
            ["PortfolioId"] = portfolioId,
            ["ClosingDate"] = closingDate.Date
        });

        static DateTime ToUtcMidnight(DateTime date)
            => date.Kind == DateTimeKind.Utc ? date.Date : DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

        const string stepName = "DistributableReturns";
        var nowUtc = DateTime.UtcNow;
        var closingDateUtc = ToUtcMidnight(closingDate);

        await timeControlService.UpdateStepAsync(portfolioId, stepName, nowUtc, cancellationToken);

        var yield = await yieldRepository.GetReadOnlyToDistributeByPortfolioAndDateAsync(portfolioId, closingDateUtc, cancellationToken);
        if (yield is null)
        {
            return Result.Failure(new Error("DR001", "No se encontraron rendimientos para la fecha de cierre indicada.", ErrorType.Failure));
        }

        var portfolioValuation = await portfolioValuationRepository.GetReadOnlyToDistributePortfolioAndDateAsync(portfolioId, closingDateUtc, cancellationToken);
        if (portfolioValuation is null)
        {
            return Result.Failure(new Error("DR002", "No existe valoración del portafolio para la fecha de cierre indicada.", ErrorType.Failure));
        }

        if (portfolioValuation.Amount <= 0m)
        {
            return Result.Failure(new Error("DR003", "El valor del portafolio es cero, no es posible calcular participaciones.", ErrorType.Failure));
        }

        var trustYields = await trustYieldRepository.GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDateUtc, cancellationToken);
        if (trustYields.Count == 0)
        {
            logger.LogInformation("[{Service}] No existen registros en rendimientos_fideicomisos para procesar.", nameof(DistributableReturnsService));
            return Result.Success();
        }

        var candidates = trustYields
            .Where(t => t.PreClosingBalance == 0m && t.TrustId != 0)
            .ToList();

        if (candidates.Count == 0)
        {
            logger.LogInformation("[{Service}] No existen fideicomisos con saldo_pre_cierre = 0.", nameof(DistributableReturnsService));
            return Result.Success();
        }

        var trustIds = candidates
            .Select(t => t.TrustId)
            .Distinct()
            .ToArray();

        var operationTypeIdResult = await GetDebitNoteOperationTypeIdAsync(operationTypesService, cancellationToken);
        if (operationTypeIdResult.IsFailure)
        {
            logger.LogError(
                "[{Service}] No se pudo resolver el tipo de operación {OperationTypeName}: {ErrorCode} - {ErrorMessage}.",
                nameof(DistributableReturnsService),
                DebitNoteOperationName,
                operationTypeIdResult.Error.Code,
                operationTypeIdResult.Error.Description);

            return Result.Failure(operationTypeIdResult.Error);
        }

        var debitNoteOperationTypeId = operationTypeIdResult.Value;

        var annulledTrustIds = await clientOperationRepository.GetTrustIdsByOperationTypeAndProcessDateAsync(
            trustIds,
            closingDateUtc,
            debitNoteOperationTypeId,
            cancellationToken);

        if (annulledTrustIds.Count == 0)
        {
            logger.LogInformation("[{Service}] No existen operaciones ND anuladas para los fideicomisos evaluados.", nameof(DistributableReturnsService));
            return Result.Success();
        }

        var conceptUuids = new[]
        {
            ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote
        };

        var concepts = await configurationParameterRepository.GetReadOnlyByUuidsAsync(
            conceptUuids,
            cancellationToken);

        var adjustmentConceptResult = ValidateConcept(
            concepts,
            ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote,
            "Ajuste Rendimiento Nota Contable");

        if (adjustmentConceptResult.IsFailure)
        {
            return Result.Failure(adjustmentConceptResult.Error);
        }

        var adjustmentConcept = adjustmentConceptResult.Value;
        var annulledSet = new HashSet<long>(annulledTrustIds);
        var applicationDateUtc = closingDateUtc;

        var rows = new List<YieldToDistribute>(capacity: annulledSet.Count);
        var trustYieldIdsToDelete = new List<long>(capacity: annulledSet.Count);

        foreach (var trustYield in candidates)
        {
            if (!annulledSet.Contains(trustYield.TrustId))
            {
                continue;
            }

            var participation = TrustMath.CalculateTrustParticipation(
                Math.Round(trustYield.ClosingBalance, DecimalPrecision.SixteenDecimals),
                portfolioValuation.Amount,
                DecimalPrecision.SixteenDecimals);

            var yieldAmountRaw = TrustMath.ApplyParticipation(
                yield.YieldToCredit,
                participation,
                DecimalPrecision.SixteenDecimals);

            var yieldAmount = MoneyHelper.Round2(yieldAmountRaw);

            var entityResult = YieldToDistribute.Create(
                trustYield.TrustId,
                portfolioId,
                closingDateUtc,
                applicationDateUtc,
                participation,
                yieldAmount,
                adjustmentConcept,
                nowUtc);

            if (entityResult.IsFailure)
            {
                return Result.Failure(entityResult.Error);
            }

            rows.Add(entityResult.Value);
            trustYieldIdsToDelete.Add(trustYield.TrustYieldId);
        }

        if (rows.Count == 0)
        {
            logger.LogInformation("[{Service}] No se generaron registros en rendimientos_por_distribuir tras aplicar los filtros.", nameof(DistributableReturnsService));
            return Result.Success();
        }

        await yieldToDistributeRepository.InsertRangeAsync(rows, cancellationToken);
        await yieldToDistributeRepository.SaveChangesAsync(cancellationToken);
        await trustYieldRepository.DeleteByIdsAsync(trustYieldIdsToDelete, cancellationToken);

        logger.LogInformation("[{Service}] Se generaron {Count} registros en rendimientos_por_distribuir.", nameof(DistributableReturnsService), rows.Count);

        return Result.Success();
    }

    private static Result<JsonDocument> ValidateConcept(
        IReadOnlyDictionary<Guid, ConfigurationParameter> concepts,
        Guid conceptUuid,
        string conceptDisplayName)
    {
        if (!concepts.TryGetValue(conceptUuid, out var parameter) || parameter.Metadata is null)
        {
            return Result.Failure<JsonDocument>(new Error(
                "DR004",
                $"No se encontró la configuración del concepto {conceptDisplayName}.",
                ErrorType.Failure));
        }

        var conceptId = JsonIntegerHelper.ExtractInt32(parameter.Metadata, "id", defaultValue: 0);
        var conceptName = JsonStringHelper.ExtractString(parameter.Metadata, "nombre", defaultValue: string.Empty);

        if (conceptId <= 0 || string.IsNullOrWhiteSpace(conceptName))
        {
            return Result.Failure<JsonDocument>(new Error(
                "DR005",
                $"La metadata del concepto {conceptDisplayName} es inválida.",
                ErrorType.Failure));
        }

        var conceptDto = new StringEntityDto(conceptId.ToString(), conceptName);
        var json = JsonSerializer.Serialize(conceptDto);
        return Result.Success(JsonDocument.Parse(json));
    }

    private static Result<long> ResolveDebitNoteOperationTypeId(IReadOnlyCollection<OperationTypeInfo> operationTypes)
    {
        var debitNoteType = operationTypes.FirstOrDefault(t =>
            DebitNoteCompareInfo.Compare(t.Name, DebitNoteOperationName, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0);

        if (debitNoteType is null || debitNoteType.OperationTypeId <= 0)
        {
            return Result.Failure<long>(new Error(
                "DR006",
                "No se encontró el tipo de operación 'Nota Débito' configurado.",
                ErrorType.Failure));
        }

        return Result.Success(debitNoteType.OperationTypeId);
    }

    private static async Task<Result<long>> GetDebitNoteOperationTypeIdAsync(
        IOperationTypesService operationTypesService,
        CancellationToken cancellationToken)
    {
        var operationTypesResult = await operationTypesService.GetAllAsync(cancellationToken);
        if (operationTypesResult.IsFailure)
        {
            return Result.Failure<long>(operationTypesResult.Error);
        }

        return ResolveDebitNoteOperationTypeId(operationTypesResult.Value);
    }
}
