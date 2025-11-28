using Common.SharedKernel.Domain;

namespace Accounting.Application.Abstractions.External;

public interface IPortfolioLocator
{
    Task<Result<PortfolioResponse>> GetPortfolioInformationAsync(int portfolioId, CancellationToken ct);
    Task<Result<bool>> AreAllPortfoliosClosedAsync(IEnumerable<int> portfolioIds, DateTime date, CancellationToken ct);
}

public sealed record PortfolioResponse
(
    string NitApprovedPortfolio,
    int VerificationDigit,
    string Name
);