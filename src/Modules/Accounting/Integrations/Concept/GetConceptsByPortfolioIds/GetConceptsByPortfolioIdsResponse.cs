namespace Accounting.Integrations.Concept.GetConceptsByPortfolioIds;

public sealed record class GetConceptsByPortfolioIdsResponse(
    int PortfolioId,
    string Name,
    string? DebitAccount,
    string? CreditAccount
);

