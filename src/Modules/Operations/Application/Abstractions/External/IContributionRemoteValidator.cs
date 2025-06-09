using Common.SharedKernel.Domain;

namespace Operations.Application.Abstractions.External;

public interface IContributionRemoteValidator
{
    Task<Result<ContributionRemoteData>> ValidateAsync(
        int activateId,
        int objectiveId,
        string? portfolioId,
        DateTime depositDate,
        DateTime execDate,
        decimal amount,
        CancellationToken ct);
}

public readonly record struct ContributionRemoteData(
    int AffiliateId,
    int ObjectiveId,
    int PortfolioId,
    string PortfolioName,
    decimal PortfolioInitialMinimumAmount);