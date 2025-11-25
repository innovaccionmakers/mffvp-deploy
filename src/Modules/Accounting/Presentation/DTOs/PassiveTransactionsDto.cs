namespace Accounting.Presentation.DTOs
{
    public sealed record class PassiveTransactionsDto(
        long PassiveTransactionId,
        string? DebitAccount,
        string? CreditAccount,
        string? ContraCreditAccount,
        string? ContraDebitAccount
        );
}
