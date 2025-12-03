namespace Accounting.Integrations.Concept.GetConcepts
{
    public sealed record class GetConceptsResponse(
        long ConceptId,
        int PortfolioId,
        string Name,
        string? DebitAccount,
        string? CreditAccount
        );
}

