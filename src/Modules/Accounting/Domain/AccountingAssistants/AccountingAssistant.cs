using Common.SharedKernel.Domain;
using HotChocolate.Types.Introspection;

namespace Accounting.Domain.AccountingAssistants;

public class AccountingAssistant : Entity
{
    public long AccountingAssistantId { get; private set; }
    public string Identification { get; private set; }
    public int? VerificationDigit { get; private set; }
    public string Name { get; private set; }
    public string? Period { get; private set; }
    public string? Account { get; private set; }
    public DateTime? Date { get; private set; }
    public string? Nit { get; private set; }
    public string? Detail { get; private set; }
    public string Type { get; private set; }
    public decimal? Value { get; private set; }
    public string Nature { get; private set; }
    public long Identifier { get; private set; }

    private AccountingAssistant()
    {
    }

    public static Result<AccountingAssistant> Create(
        int? verificationDigit,
        string name,
        string? period,
        string? account,
        DateTime? date,
        string? nit,
        string? detail,
        string type,
        decimal? value,
        string nature,
        long identifier)
    {
        var accountingAssistant = new AccountingAssistant
        {
            AccountingAssistantId = default,
            VerificationDigit = verificationDigit,
            Name = name,
            Period = period,
            Account = account,
            Date = date,
            Nit = nit,
            Detail = detail,
            Type = type,
            Value = value,
            Nature = nature,
            Identifier = identifier
        };
        return Result.Success(accountingAssistant);
    }

    public void UpdateDetails(
        int? verificationDigit,
        string name,
        string? period,
        string? account,
        DateTime? date,
        string? nit,
        string? detail,
        string type,
        decimal? value,
        string nature,
        long identifier)
    {
        VerificationDigit = verificationDigit;
        Name = name;
        Period = period;
        Account = account;
        Date = date;
        Nit = nit;
        Detail = detail;
        Type = type;
        Value = value;
        Nature = nature;
        Identifier = identifier;
    }
}
