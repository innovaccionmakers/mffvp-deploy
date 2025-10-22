using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Operations.Application.Abstractions.Data;
using Operations.Application.Abstractions.External;
using Operations.Application.Abstractions.Services.OperationCompleted;
using Operations.Application.Abstractions.Services.Voids;
using Operations.Domain.ClientOperations;

namespace Operations.Application.Voids.Services;

public sealed class VoidedTransactionsOper(
    IUnitOfWork unitOfWork,
    IClientOperationRepository clientOperationRepository,
    ITrustUpdater trustUpdater,
    IOperationCompleted operationCompleted)
    : IVoidsOper
{
    private const string BaseSuccessMessage = "Anulación registrada. {0} operación(es) anulada(s). Operación(es) en proceso de cierre.";

    public async Task<Result<VoidedTransactionsOperResult>> ExecuteAsync(
        VoidedTransactionsOperRequest request,
        VoidedTransactionsValidationResult validationResult,
        CancellationToken cancellationToken)
    {
        var annulledOperationIds = new List<long>();

        foreach (var operation in validationResult.ValidOperations)
        {
            await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            var original = operation.OriginalOperation;

            var causeId = validationResult.CauseConfigurationParameterId == 0
                ? original.CauseId
                : validationResult.CauseConfigurationParameterId;

            original.UpdateDetails(
                original.RegistrationDate,
                original.AffiliateId,
                original.ObjectiveId,
                original.PortfolioId,
                original.Amount,
                original.ProcessDate,
                original.OperationTypeId,
                original.ApplicationDate,
                LifecycleStatus.Annulled,
                causeId,
                operation.TrustId,
                original.LinkedClientOperationId,
                original.Units);

            clientOperationRepository.Update(original);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var update = new TrustUpdate(
                original.ClientOperationId,
                LifecycleStatus.Annulled,
                0m,
                0m,
                0m,
                DateTime.UtcNow);

            var trustUpdateResult = await trustUpdater
                .UpdateAsync(update, cancellationToken);

            if (trustUpdateResult.IsFailure)
            {
                return Result.Failure<VoidedTransactionsOperResult>(trustUpdateResult.Error);
            }

            annulledOperationIds.Add(original.ClientOperationId);

            await operationCompleted.ExecuteAsync(original, cancellationToken);
        }

        var message = BuildSuccessMessage(annulledOperationIds);

        return Result.Success(new VoidedTransactionsOperResult(annulledOperationIds, message));
    }

    private static string BuildSuccessMessage(IReadOnlyCollection<long> annulledOperationIds)
    {
        var baseMessage = string.Format(BaseSuccessMessage, annulledOperationIds.Count);

        if (annulledOperationIds.Count == 0)
        {
            return baseMessage;
        }

        var ids = string.Join(",", annulledOperationIds);

        return $"{baseMessage} Operación(es) anulada(s): {ids}.";
    }
}
