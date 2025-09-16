using Common.SharedKernel.Domain;

namespace Accounting.Domain.Treasuries;

public sealed class Treasury : Entity
{
    public long TreasuryId { get; private set; }
    public int PortfolioId { get; private set; }
    public string? BankAccount { get; private set; }
    public string? DebitAccount { get; private set; }
    public string? CreditAccount { get; private set; }

    private Treasury()
    {
    }

    public static Result<Treasury> Create(
        int portfolioId,
        string? bankAccount,
        string? debitAccount,
        string? creditAccount)
    {
        var treasury = new Treasury
        {
            TreasuryId = default,
            PortfolioId = portfolioId,
            BankAccount = bankAccount,
            DebitAccount = debitAccount,
            CreditAccount = creditAccount
        };
        return Result.Success(treasury);
    }

    public void UpdateDetails(
        int portfolioId,
        string? bankAccount,
        string? debitAccount,
        string? creditAccount)
    {
        PortfolioId = portfolioId;
        BankAccount = bankAccount;
        DebitAccount = debitAccount;
        CreditAccount = creditAccount;
    }
}
