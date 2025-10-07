
using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Products.AccumulatedCommissions;

public sealed record UpdateAccumulatedCommissionRemoteRequest(
    int PortfolioId,
    int CommissionId,
    decimal AccumulatedValue,
    DateTime ClosingDateUtc,
    string Origin,             // "Closing"
    string? IdempotencyKey = null,
    string? ExecutionId = null 
);

public sealed record UpdateAccumulatedCommissionRemoteResponse(
    bool Succeeded,
    string Status,             // "Updated" | "NoChange" | "Error"
    string? Message = null
);

public interface IUpdateAccumulatedCommissionRemote
{
    Task<Result<UpdateAccumulatedCommissionRemoteResponse>> ExecuteAsync(
        UpdateAccumulatedCommissionRemoteRequest request,
        CancellationToken cancellationToken);
}