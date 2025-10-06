using Common.SharedKernel.Domain;

namespace Closing.Application.Abstractions.External.Products.Portfolios;

public sealed record UpdatePortfolioFromClosingRemoteRequest(
    int PortfolioId,
    DateTime ClosingDateUtc,
    string IdempotencyKey,
    string Origin,            // <-- agregado: "Closing"
    string? ExecutionId = null
);

public sealed record UpdatePortfolioFromClosingRemoteResponse(
    bool Succeeded,
    string Status,        // "Updated" | "NoChange" | "Error"
    int UpdatedCount,
    string? Message = null
);

public interface IUpdatePortfolioFromClosingRemote
{
    Task<Result<UpdatePortfolioFromClosingRemoteResponse>> ExecuteAsync(
        UpdatePortfolioFromClosingRemoteRequest request,
        CancellationToken cancellationToken);
}
