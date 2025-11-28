namespace Accounting.Integrations.PassiveTransaction.GetPassiveTransactions
{
    public sealed record class GetPassiveTransactionsResponse(
        long PassiveTransactionId,
        string? DebitAccount,
        string? CreditAccount,
        string? ContraCreditAccount,
        string? ContraDebitAccount
        );
}
