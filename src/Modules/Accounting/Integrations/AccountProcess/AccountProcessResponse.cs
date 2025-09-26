namespace Accounting.Integrations.AccountProcess
{
    public sealed record class AccountProcessResponse(
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
