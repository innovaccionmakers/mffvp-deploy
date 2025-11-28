using Common.SharedKernel.Domain;

namespace Accounting.Application.Abstractions.External;

public interface IYieldDetailsLocator
{
    Task<Result<IReadOnlyCollection<YieldDetailResponse>>> GetYieldsDetailsByPortfolioIdsClosingDateSourceAndConceptAsync(IEnumerable<int> portfolioIds, DateTime closingDate, string source, string? concept, CancellationToken ct);
}

public sealed record YieldDetailResponse
(
    long YieldDetailId,
    int PortfolioId,    
    decimal Income,
    decimal Expenses,
    decimal Commissions,    
    DateTime ClosingDate,
    DateTime ProcessDate,
    bool IsClosed
);
