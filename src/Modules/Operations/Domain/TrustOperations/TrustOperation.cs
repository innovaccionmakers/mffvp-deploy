using Common.SharedKernel.Domain;
using Operations.Domain.ClientOperations;

namespace Operations.Domain.TrustOperations;

public sealed class TrustOperation : Entity
{
    public long TrustOperationId { get; private set; }
    public long? ClientOperationId { get; private set; }
    public long TrustId { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Units { get; private set; }
    public long OperationTypeId { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public DateTime ApplicationDate { get; private set; }
    public decimal WithdrawalContingentTax { get; private set; }
    public decimal WithdrawalContributionsTax { get; private set; }
    public decimal AmountRequested { get; private set; }
    public decimal ContributionsPaid { get; private set; }
    public decimal PaidCapital { get; private set; }

    public ClientOperation ClientOperation { get; private set; } = null!;

    private TrustOperation()
    {
    }

    public static Result<TrustOperation> Create(
        long? clientOperationId,
        long trustId,
        decimal amount,
        decimal units,
        long operationTypeId,
        int portfolioId,
        DateTime registrationDate,
        DateTime processDate,
        DateTime applicationDate,
        decimal withdrawalContingentTax = 0,
        decimal withdrawalContributionsTax = 0,
        decimal amountRequested = 0,
        decimal contributionsPaid = 0,
        decimal paidCapital = 0
    )
    {
        var trustOperation = new TrustOperation
        {
            TrustOperationId = default,
            ClientOperationId = clientOperationId,
            TrustId = trustId,
            Amount = amount,
            Units = units,
            OperationTypeId = operationTypeId,
            PortfolioId = portfolioId,
            RegistrationDate = registrationDate,
            ProcessDate = processDate,
            ApplicationDate = applicationDate,
            WithdrawalContingentTax = withdrawalContingentTax,
            WithdrawalContributionsTax = withdrawalContributionsTax,
            AmountRequested = amountRequested,
            ContributionsPaid = contributionsPaid,
            PaidCapital = paidCapital
        };

        return Result.Success(trustOperation);
    }

    public void UpdateDetails(
        long? newClientOperationId,
        long newTrustId,
        decimal newAmount,
        decimal newUnits,
        long newOperationTypeId,
        int newPortfolioId,
        DateTime newRegistrationDate,
        DateTime newProcessDate,
        DateTime newApplicationDate,
        decimal newWithdrawalContingentTax = 0,
        decimal newWithdrawalContributionsTax = 0,
        decimal newAmountRequested = 0,
        decimal newContributionsPaid = 0,
        decimal newPaidCapital = 0
    )
    {
        ClientOperationId = newClientOperationId;
        TrustId = newTrustId;
        Amount = newAmount;
        Units = newUnits;
        OperationTypeId = newOperationTypeId;
        PortfolioId = newPortfolioId;
        RegistrationDate = newRegistrationDate;
        ProcessDate = newProcessDate;
        ApplicationDate = newApplicationDate;
        WithdrawalContingentTax = newWithdrawalContingentTax;
        WithdrawalContributionsTax = newWithdrawalContributionsTax;
        AmountRequested = newAmountRequested;
        ContributionsPaid = newContributionsPaid;
        PaidCapital = newPaidCapital;
    }
}