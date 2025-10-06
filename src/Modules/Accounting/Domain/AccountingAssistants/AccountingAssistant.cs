using Common.SharedKernel.Domain;
using Common.SharedKernel.Core.Primitives;
namespace Accounting.Domain.AccountingAssistants;
using Accounting.Domain.Constants;

public class AccountingAssistant : Entity, ICloneable
{
    public long AccountingAssistantId { get; private set; }
    public string Identification { get; private set; }
    public int? VerificationDigit { get; private set; }
    public string Name { get; private set; }
    public string? Period { get; private set; }
    public string? Account { get; private set; }
    public DateTime? Date { get; private set; }
    public string? Detail { get; private set; }
    public string Type { get; private set; }
    public decimal? Value { get; private set; }
    public string Nature { get; private set; }
    public Guid Identifier { get; private set; }

    private AccountingAssistant()
    {
    }

    public static Result<AccountingAssistant> Create(
        string identification,
        int? verificationDigit,
        string name,
        string? period,
        DateTime? date,
        string? detail,
        decimal? value,
        string nature,
        string type = "",
        string? account = "")
    {
        var accountingAssistant = new AccountingAssistant
        {
            AccountingAssistantId = default,
            Identification = identification,
            VerificationDigit = verificationDigit,
            Name = name,
            Period = period,
            Account = account,
            Date = date?.ToUniversalTime(),
            Detail = detail,
            Type = type,
            Value = value,
            Nature = nature,
            Identifier = Guid.NewGuid()
        };

        var validationResult = accountingAssistant.ValidateRequiredFields();
        if (validationResult.IsFailure)
        {
            return Result.Failure<AccountingAssistant>(validationResult.Error);
        }

        return Result.Success(accountingAssistant);
    }

    public void UpdateDetails(
        string identification,
        int? verificationDigit,
        string name,
        string? period,
        DateTime? date,
        string? detail,
        decimal? value,
        string nature,
        string type = "",
        string? account = "")
    {
        Identification = identification;
        VerificationDigit = verificationDigit;
        Name = name;
        Period = period;
        Account = account;
        Date = date;
        Detail = detail;
        Type = type;
        Value = value;
        Nature = nature;
    }

    public AccountingAssistant DuplicateWithType(string type, string? account = null)
    {
        var clone = (AccountingAssistant)Clone();
        clone.AccountingAssistantId = default;
        clone.Type = type;
        if (account != null) clone.Account = account;
        return clone;
    }

    public IEnumerable<AccountingAssistant> ToDebitAndCredit(string? debitAccount = null, string? creditAccount= null)
    {
        yield return DuplicateWithType(AccountingTypes.Debit, debitAccount);
        yield return DuplicateWithType(AccountingTypes.Credit, creditAccount);
    }


    public Result ValidateRequiredFields()
    {
        var errors = new List<Error>();

        var detailValidation = ValidateDetail();
        if (detailValidation.IsFailure)
        {
            errors.Add(detailValidation.Error);
        }

        if (errors.Count != 0)
        {
            return Result.Failure<bool>(new ValidationError(errors.ToArray()));
        }

        return Result.Success(true);
    }

    private Result ValidateDetail()
    {
        if (string.IsNullOrWhiteSpace(Detail))
        {
            return Result.Failure<bool>(Error.Validation(
                "AccountingAssistant.Detail.Required",
                $"El detalle es obligatorio para la entidad {Identifier}"));
        }

        return Result.Success(true);
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}
