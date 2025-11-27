using System.Text.Json;
using Common.SharedKernel.Domain;

namespace Accounting.Application.Abstractions.External;

public interface IYieldToDistributeLocator
{
    public Task<Result<IReadOnlyCollection<DistributedYieldGroupResponse>>> GetDistributedYieldGroupResponse(IEnumerable<int> portfolioIds,
                                                                                                     DateTime closingDate,
                                                                                                     string concept,
                                                                                                     CancellationToken ct);  
}
public sealed record DistributedYieldGroupResponse
(
    DateTime ClosinDate,
    int PortfolioId,
    JsonDocument Concept,
    decimal TotalYieldAmount
);
