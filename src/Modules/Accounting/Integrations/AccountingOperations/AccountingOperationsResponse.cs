namespace Accounting.Integrations.AccountingOperations
{
    public sealed record class AccountingOperationsResponse(
        string Identification,
        int? VerificationDigit,
        string Name,
        string? Period,
        string? Account,
        DateTime? Date,
        string? Nit,
        string? Detail,
        string Type,
        decimal? Value,
        string Nature,
        long Identifier
        );
}
