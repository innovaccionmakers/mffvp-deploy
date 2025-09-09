using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.External;

public interface IPortfolioLocator
{
    Task<Result<(long PortfolioId, string Name, DateTime CurrentDate)>> FindByPortfolioIdAsync(int PortfolioId, CancellationToken ct);
    Task<Result<(long PortfolioId, string Name, DateTime CurrentDate)>> FindByHomologatedCodeAsync(string homologatedCodePortfolio, CancellationToken ct);
}
