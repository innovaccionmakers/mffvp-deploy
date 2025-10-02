namespace Accounting.Integrations.Treasuries.GetAccountingConceptsTreasuries
{
    public sealed record class GetAccountingConceptsTreasuriesResponse(
        int PortfolioId,
        string? DebitAccount
        );
}
