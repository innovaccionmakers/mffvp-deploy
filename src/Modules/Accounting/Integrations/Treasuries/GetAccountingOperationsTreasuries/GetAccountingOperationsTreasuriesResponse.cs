namespace Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries
{
    public sealed record class GetAccountingOperationsTreasuriesResponse(
        int PortfolioId,
        string? DebitAccount
        );
}
