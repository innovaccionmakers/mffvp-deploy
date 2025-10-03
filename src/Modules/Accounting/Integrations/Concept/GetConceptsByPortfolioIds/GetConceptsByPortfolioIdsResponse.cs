namespace Accounting.Integrations.Concept.GetConceptsByPortfolioIds;

namespace Accounting.Integrations.Treasuries.GetConceptsByPortfolioIds
{
    public sealed record class GetConceptsByPortfolioIdsResponse(
        int PortfolioId,
        string? DebitAccount,
        string? CreditAccount
        );
}
