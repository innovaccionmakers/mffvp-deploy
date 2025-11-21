using Common.SharedKernel.Domain;

namespace Accounting.Application.Abstractions.External;

public interface IYieldDetailsLocator
{
    Task<Result<IReadOnlyCollection<YieldDetailResponse>>> GetYieldsDetailsByPortfolioIdsClosingDateAndSourceAsync(IEnumerable<int> portfolioIds, string source, DateTime closingDate, CancellationToken ct);
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
