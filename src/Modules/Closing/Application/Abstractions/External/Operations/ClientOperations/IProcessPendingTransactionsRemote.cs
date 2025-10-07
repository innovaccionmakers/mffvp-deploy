

using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Operations.ClientOperations;

public interface IProcessPendingTransactionsRemote
{
    Task<Result<ProcessPendingTransactionsRemoteResponse>> ExecuteAsync(
        ProcessPendingTransactionsRemoteRequest request,
        CancellationToken cancellationToken = default);
}
public sealed record ProcessPendingTransactionsRemoteRequest(
    int PortfolioId,
    DateTime ProcessDateUtc,
    string? IdempotencyKey = null,     // ej: pendingtx:{portfolioId}:{yyyyMMdd}
    string? ExecutionId = null // opcional
);

public sealed record ProcessPendingTransactionsRemoteResponse(
    bool Succeeded,
    string Status,             // "Processed" | "NothingToProcess" | "Error"
    int AppliedCount,
    int SkippedCount,
    string? Message = null
);