using Common.SharedKernel.Domain;
using Treasury.Domain.Issuers;
using Treasury.Domain.TreasuryMovements;

namespace Treasury.Domain.BankAccounts;

public sealed class BankAccount : Entity
{
    private BankAccount()
    {
    }

    public long Id { get; private set; }
    public long PortfolioId { get; private set; }
    public long IssuerId { get; private set; }
    public string AccountNumber { get; private set; }
    public string AccountType { get; private set; }
    public string Observations { get; private set; }
    public DateTime ProcessDate { get; private set; }

    public Issuer Issuer { get; set; }
    public ICollection<TreasuryMovement> TreasuryMovements { get; set; }

    public static Result<BankAccount> Create(
        long portfolioId,
        long issuerId,
        string accountNumber,
        string accountType,
        string observations,
        DateTime processDate)
    {
        var bankAccount = new BankAccount
        {
            PortfolioId = portfolioId,
            IssuerId = issuerId,
            AccountNumber = accountNumber,
            AccountType = accountType,
            Observations = observations,
            ProcessDate = processDate
        };

        return Result.Success(bankAccount);
    }
}