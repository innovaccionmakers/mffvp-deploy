using Common.SharedKernel.Domain;

namespace Accounting.Application.Abstractions.External;

public interface IYieldDetailsLocator
{
    Task<Result<IReadOnlyCollection<YieldDetailResponse>>> GetYieldsDetailsByPortfolioIdsClosingDateSourceAndConceptAsync(IEnumerable<int> portfolioIds, DateTime closingDate, string source, Guid? duidConcept, CancellationToken ct);
    Task<Result<IReadOnlyCollection<YieldDetailResponse>>> GetYieldDetailsByPortfolioIdsAndClosingDateAsync(IEnumerable<int> portfolioIds, DateTime closingDate, string source, CancellationToken ct);
    Task<Result<IReadOnlyCollection<YieldDetailResponse>>> GetYieldDetailsByPortfolioIdsAndClosingDateWithConceptAsync(IEnumerable<int> portfolioIds, DateTime closingDate, string source, Guid guidConcept, CancellationToken ct);
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
