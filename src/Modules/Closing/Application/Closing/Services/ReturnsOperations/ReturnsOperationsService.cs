using Closing.Application.Closing.Services.ReturnsOperations.Interfaces;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.PreClosing.Services.AutomaticConcepts.Dto;
using Closing.Application.PreClosing.Services.Yield;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Application.PreClosing.Services.Yield.Interfaces;
using Closing.Domain.ConfigurationParameters;
using Closing.Domain.TrustYields;
using Closing.Domain.YieldsToDistribute;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.Serialization;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Domain.ConfigurationParameters;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Closing.Application.Closing.Services.ReturnsOperations;

public sealed class ReturnsOperationsService(
    IYieldToDistributeRepository yieldToDistributeRepository,
    IConfigurationParameterRepository configurationParameterRepository,
    IYieldDetailCreationService yieldDetailCreationService,
    YieldDetailBuilderService yieldDetailBuilderService,
    ITimeControlService timeControlService,
    ITrustYieldRepository trustYieldRepository,
    ILogger<ReturnsOperationsService> logger)
    : IReturnsOperationsService
{
    private const string StepName = "ReturnsOperations";

    public async Task<Result> RunAsync(
        int portfolioId,
        DateTime valuationDate,
        bool isInternalProcess,
        CancellationToken cancellationToken)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["Service"] = nameof(ReturnsOperationsService),
            ["PortfolioId"] = portfolioId,
            ["ValuationDate"] = valuationDate.Date
        });

        static DateTime Normalize(DateTime date)
            => date.Kind == DateTimeKind.Utc ? date.Date : DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

        var nowUtc = DateTime.UtcNow;
        var valuationDateUtc = Normalize(valuationDate);
        var nextClosingDateUtc = valuationDateUtc.AddDays(1);

        await timeControlService.UpdateStepAsync(portfolioId, StepName, nowUtc, cancellationToken);

        var configurationResult = await GetRequiredConceptsAsync(configurationParameterRepository, cancellationToken);
        if (configurationResult.IsFailure)
        {
            return Result.Failure(configurationResult.Error);
        }

        var concepts = configurationResult.Value;

        var rows = await yieldToDistributeRepository
            .GetReadOnlyByPortfolioAndDateAsync(portfolioId, valuationDateUtc, cancellationToken);

        if (rows.Count == 0)
        {
            logger.LogInformation("No existen registros en rendimientos_por_distribuir para procesar.");
            return Result.Success();
        }

        var matchingRows = new List<YieldToDistribute>(rows.Count);
        var trustIdsToCleanup = new HashSet<long>();
        foreach (var row in rows)
        {
            var conceptIdResult = ExtractConceptId(row.Concept);
            if (conceptIdResult.IsFailure)
            {
                return Result.Failure(conceptIdResult.Error);
            }

            if (conceptIdResult.Value == concepts.CreditNote.Id)
            {
                matchingRows.Add(row);
                trustIdsToCleanup.Add(row.TrustId);
            }
        }

        if (matchingRows.Count == 0)
        {
            logger.LogInformation("No se encontraron rendimientos pendientes con concepto de nota contable de ajuste.");
            return Result.Success();
        }

        var positiveTotal = matchingRows.Where(x => x.YieldAmount > 0m).Sum(x => x.YieldAmount);
        var negativeTotal = matchingRows.Where(x => x.YieldAmount < 0m).Sum(x => x.YieldAmount);

        var summaries = new List<AutomaticConceptSummary>(capacity: 2);

        if (positiveTotal > 0m)
        {
            summaries.Add(new AutomaticConceptSummary(
                concepts.Income.Id,
                concepts.Income.Name,
                IncomeExpenseNature.Income,
                YieldsSources.AutomaticConcept,
                positiveTotal));
        }

        if (negativeTotal < 0m)
        {
            summaries.Add(new AutomaticConceptSummary(
                concepts.Expense.Id,
                concepts.Expense.Name,
                IncomeExpenseNature.Expense,
                YieldsSources.AutomaticConcept,
                negativeTotal));
        }

        if (summaries.Count == 0)
        {
            logger.LogInformation("El agregado de rendimientos por distribuir es cero para el portafolio {PortfolioId}.", portfolioId);
            return Result.Success();
        }

        var parameters = new RunSimulationParameters(
            PortfolioId: portfolioId,
            ClosingDate: nextClosingDateUtc,
            IsClosing: isInternalProcess);

        var yieldDetails = yieldDetailBuilderService.Build(summaries, parameters);
        await yieldDetailCreationService.CreateYieldDetailsAsync(yieldDetails, PersistenceMode.Transactional, cancellationToken);

        if (trustIdsToCleanup.Count > 0)
        {
            var trustYields = await trustYieldRepository
                .GetReadOnlyByTrustIdsAndDateAsync(trustIdsToCleanup, valuationDateUtc, cancellationToken);

            var trustYieldIdsToDelete = trustYields.Values
                .Where(x => x.PortfolioId == portfolioId)
                .Select(x => x.TrustYieldId)
                .Distinct()
                .ToArray();

            if (trustYieldIdsToDelete.Length > 0)
            {
                var deletedCount = await trustYieldRepository.DeleteByIdsAsync(trustYieldIdsToDelete, cancellationToken);

                logger.LogInformation(
                    "Se eliminaron {Count} registros en rendimientos_fideicomisos para el portafolio {PortfolioId}.",
                    deletedCount,
                    portfolioId);
            }
            else
            {
                logger.LogInformation(
                    "No se encontraron registros en rendimientos_fideicomisos para eliminar en el portafolio {PortfolioId}.",
                    portfolioId);
            }
        }

        logger.LogInformation(
            "Se generaron {Count} registros en detalle_rendimientos para el portafolio {PortfolioId}.",
            yieldDetails.Count,
            portfolioId);

        return Result.Success();
    }

    private static async Task<Result<ConceptMetadataCollection>> GetRequiredConceptsAsync(
        IConfigurationParameterRepository repository,
        CancellationToken cancellationToken)
    {
        var uuids = new[]
        {
            ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote,
            ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteIncome,
            ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteExpense
        };

        var map = await repository.GetReadOnlyByUuidsAsync(uuids, cancellationToken);

        var creditNoteResult = ExtractConcept(map, ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNote, "RO001",
            "No se encontró la configuración del concepto de nota contable de rendimientos.");
        if (creditNoteResult.IsFailure)
        {
            return Result.Failure<ConceptMetadataCollection>(creditNoteResult.Error);
        }

        var incomeResult = ExtractConcept(map, ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteIncome, "RO002",
            "No se encontró la configuración del concepto 'Ajuste Rendimiento NC Ingreso'.");
        if (incomeResult.IsFailure)
        {
            return Result.Failure<ConceptMetadataCollection>(incomeResult.Error);
        }

        var expenseResult = ExtractConcept(map, ConfigurationParameterUuids.Closing.YieldAdjustmentCreditNoteExpense, "RO003",
            "No se encontró la configuración del concepto 'Ajuste Rendimiento NC Gasto'.");
        if (expenseResult.IsFailure)
        {
            return Result.Failure<ConceptMetadataCollection>(expenseResult.Error);
        }

        return Result.Success(new ConceptMetadataCollection(
            creditNoteResult.Value,
            incomeResult.Value,
            expenseResult.Value));
    }

    private static Result<ConceptMetadata> ExtractConcept(
        IReadOnlyDictionary<Guid, ConfigurationParameter> map,
        Guid uuid,
        string errorCode,
        string errorMessage)
    {
        if (!map.TryGetValue(uuid, out var parameter) || parameter.Metadata is null)
        {
            return Result.Failure<ConceptMetadata>(Error.Failure(errorCode, errorMessage));
        }

        var id = JsonIntegerHelper.ExtractInt32(parameter.Metadata, "id", defaultValue: 0);
        var name = JsonStringHelper.ExtractString(parameter.Metadata, "nombre", defaultValue: string.Empty);

        if (id <= 0 || string.IsNullOrWhiteSpace(name))
        {
            return Result.Failure<ConceptMetadata>(Error.Failure(errorCode, errorMessage));
        }

        return Result.Success(new ConceptMetadata(id, name));
    }

    private static Result<int> ExtractConceptId(JsonDocument concept)
    {
        if (concept is null)
        {
            return Result.Failure<int>(Error.Failure("RO004", "No se encontró metadata del concepto de la nota contable."));
        }

        var root = concept.RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return Result.Failure<int>(Error.Failure("RO004", "El concepto de la nota contable tiene un formato inválido."));
        }

        if (TryReadInt(root, "id", out var conceptId) || TryReadInt(root, "EntityId", out conceptId))
        {
            return Result.Success(conceptId);
        }

        return Result.Failure<int>(Error.Failure("RO004", "No se pudo determinar el identificador del concepto de la nota contable."));
    }

    private static bool TryReadInt(JsonElement root, string propertyName, out int value)
    {
        value = 0;
        if (!root.TryGetProperty(propertyName, out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out value))
        {
            return true;
        }

        if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out value))
        {
            return true;
        }

        return false;
    }

    private sealed record ConceptMetadata(int Id, string Name);

    private sealed record ConceptMetadataCollection(ConceptMetadata CreditNote, ConceptMetadata Income, ConceptMetadata Expense);
}
