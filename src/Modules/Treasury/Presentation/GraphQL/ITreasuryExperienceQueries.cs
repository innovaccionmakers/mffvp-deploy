using Treasury.Presentation.DTOs;

namespace Treasury.Presentation.GraphQL;

public interface ITreasuryExperienceQueries
{
    Task<IReadOnlyCollection<IssuerDto>> GetIssuersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BankAccountDto>> GetBankAccountsByPortfolioAsync(
        long portfolioId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<TreasuryConceptDto>> GetTreasuryConceptsAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<BankAccountDto>> GetBankAccountsByPortfolioAndIssuerAsync(
        long portfolioId,
        long issuerId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<TreasuryMovementDto>> GetTreasuryMovementsByPortfolioIdsAsync(
        IEnumerable<long> portfolioIds,
        CancellationToken cancellationToken = default);
}