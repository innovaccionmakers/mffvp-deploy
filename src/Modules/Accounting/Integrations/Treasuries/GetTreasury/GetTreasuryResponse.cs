namespace Accounting.Integrations.Treasury.GetTreasuries
{
    public sealed record class GetTreasuryResponse(
        long TreasuryId,
        string? BankAccount,
        string? DebitAccount,
        string? CreditAccount
        );
}
