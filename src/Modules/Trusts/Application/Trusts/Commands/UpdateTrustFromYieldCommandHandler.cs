using Common.SharedKernel.Application.Helpers.Money;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.Trusts;
using Trusts.Domain.Trusts.TrustYield;
using Trusts.Integrations.TrustYields.Commands;

namespace Trusts.Application.Trusts.Commands;

internal sealed class UpdateTrustFromYieldCommandHandler(
    ITrustBulkRepository trustBulkRepository,
    IUnitOfWork unitOfWork,
    ILogger<UpdateTrustFromYieldCommandHandler> logger)
    : ICommandHandler<UpdateTrustFromYieldCommand, ApplyYieldBulkBatchResult>
{
    public async Task<Result<ApplyYieldBulkBatchResult>> Handle(
        UpdateTrustFromYieldCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (request.Rows is null || request.Rows.Count == 0)
            {
                logger.LogInformation("UpdateTrustFromYield: lote {BatchIndex} sin filas para procesar.", request.BatchIndex);
                return Result.Success(
                    new ApplyYieldBulkBatchResult(
                        BatchIndex: request.BatchIndex,
                        Updated: 0,
                        MissingTrustIds: Array.Empty<long>(),
                        ValidationMismatchTrustIds: Array.Empty<long>()));
            }

            // Normalización de entradas (redondeo consistente)
            var normalizedRows = request.Rows
                .Select(r => new ApplyYieldRow(
                    r.TrustId,
                    MoneyHelper.Round2(r.YieldAmount),
                    MoneyHelper.Round2(r.YieldRetention),
                    MoneyHelper.Round2(r.ClosingBalance)))
                .ToList();

            await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            // Procesa el chunk tal cual lo envió Closing
            var repoResult = await trustBulkRepository.ApplyYieldToBalanceBulkAsync(
                normalizedRows,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            logger.LogInformation(
                "UpdateTrustFromYield: lote {BatchIndex} procesado → updated: {Updated}, missing: {Missing}, mismatch: {Mismatch}.",
                request.BatchIndex,
                repoResult.Updated,
                repoResult.MissingTrustIds?.Count ?? 0,
                repoResult.ValidationMismatchTrustIds?.Count ?? 0);

            return Result.Success(
                new ApplyYieldBulkBatchResult(
                    BatchIndex: request.BatchIndex,
                    Updated: repoResult.Updated,
                    MissingTrustIds: repoResult.MissingTrustIds,
                    ValidationMismatchTrustIds: repoResult.ValidationMismatchTrustIds));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "UpdateTrustFromYield: error inesperado al procesar el lote {BatchIndex}.", request.BatchIndex);

            return Result.Failure<ApplyYieldBulkBatchResult>(
                new Error("TRU-BULK-001",
                    "Ocurrió un error al aplicar rendimientos a los fideicomisos para el lote.",
                    ErrorType.Failure));
        }
    }
}