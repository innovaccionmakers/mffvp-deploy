using Common.SharedKernel.Domain;
using Treasury.Domain.Issuers;
using Treasury.Domain.TreasuryMovements;

namespace Treasury.Domain.BankAccounts;

public sealed class BankAccountQueryResponse
{
    public long Id { get; private set; }
    public long PortfolioId { get; private set; }
    public string PortfolioName { get; private set; }
    public long IssuerId { get; private set; }
    public string AccountNumber { get; private set; }
    public string AccountType { get; private set; }
    public string Observations { get; private set; }
    public DateTime ProcessDate { get; private set; }

    public Issuer Issuer { get; set; }
    public ICollection<TreasuryMovement> TreasuryMovements { get; set; }

    public BankAccountQueryResponse()
    {
    }

    public static Result<BankAccountQueryResponse> Create(
        long portfolioId,
        string portfolioName,
        long issuerId,
        string accountNumber,
        string accountType,
        string observations,
        DateTime processDate)
    {
        var bankAccount = new BankAccountQueryResponse
        {
            PortfolioId = portfolioId,
            PortfolioName = portfolioName,
            IssuerId = issuerId,
            AccountNumber = accountNumber,
            AccountType = accountType,
            Observations = observations,
            ProcessDate = processDate
        };

        return Result.Success(bankAccount);
    }
}