using Common.SharedKernel.Domain;

namespace Accounting.Application.Abstractions.External;

public interface IYieldLocator
{
    Task<Result<IReadOnlyCollection<YieldResponse>>> GetYieldsPortfolioIdsAndClosingDate(List<int> portfolioIds, DateTime closingDate, CancellationToken ct);
}

public sealed record YieldResponse
(
    long YieldId,
    int PortfolioId,
    decimal Income,
    decimal Expenses,
    decimal Commissions,
    decimal Costs,
    decimal YieldToCredit,
    decimal CreditedYields,
    DateTime ClosingDate,
    DateTime ProcessDate,
    bool IsClosed
);
