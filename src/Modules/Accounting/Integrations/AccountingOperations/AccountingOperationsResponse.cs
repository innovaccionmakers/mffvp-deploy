namespace Accounting.Integrations.AccountingOperations
{
    public sealed record class AccountingOperationsResponse(
        string Identification,
        int VerificationDigit,
        string FullName,
        string Period,
        string Account,
        DateTime Date,
        string Details,
        string Type,
        decimal Amount,
        string Nature,
        string Nit = "",
        long? Identificator = 0
        );
}
