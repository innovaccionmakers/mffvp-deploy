namespace Accounting.Integrations.PassiveTransaction.GetPassiveTransaction
{
    public sealed record class GetPassiveTransactionResponse(
        long PassiveTransactionId,
        string? DebitAccount,
        string? CreditAccount,
        string? ContraCreditAccount,
        string? ContraDebitAccount
        );
}
