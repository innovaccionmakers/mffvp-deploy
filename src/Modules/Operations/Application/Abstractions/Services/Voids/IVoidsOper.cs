using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.Services.Voids;

public interface IVoidsOper
{
    Task<Result<VoidedTransactionsOperResult>> ExecuteAsync(
        VoidedTransactionsOperRequest request,
        VoidedTransactionsValidationResult validationResult,
        CancellationToken cancellationToken);
}

public sealed record VoidedTransactionsOperRequest(int AffiliateId, int ObjectiveId);

public sealed record VoidedTransactionsOperResult(
    IReadOnlyCollection<long> VoidIds,
    string Message);
