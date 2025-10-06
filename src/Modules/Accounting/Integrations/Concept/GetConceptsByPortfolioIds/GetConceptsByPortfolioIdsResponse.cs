namespace Accounting.Integrations.Concept.GetConceptsByPortfolioIds;

public sealed record class GetConceptsByPortfolioIdsResponse(
    int PortfolioId,
    string? DebitAccount,
    string? CreditAccount
);

