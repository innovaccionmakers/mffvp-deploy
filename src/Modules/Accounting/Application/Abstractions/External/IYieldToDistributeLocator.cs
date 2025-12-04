using Common.SharedKernel.Domain;

namespace Accounting.Application.Abstractions.External;

public interface IYieldToDistributeLocator
{
    public Task<Result<IReadOnlyCollection<GenericDebitNoteResponse>>> GetDistributedYieldGroupResponse(IEnumerable<int> portfolioIds,
                                                                                                     DateTime closingDate,
                                                                                                     CancellationToken ct);  
}

public sealed record GenericDebitNoteResponse
(
    DateTime ClosinDate,
    int PortfolioId,
    decimal Value
);
