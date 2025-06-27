using Common.SharedKernel.Domain;
using Operations.Domain.ClientOperations;

namespace Operations.Domain.TrustOperations;

public sealed class TrustOperation : Entity
{
    public long TrustOperationId { get; private set; }
    public long ClientOperationId { get; private set; }
    public long TrustId { get; private set; }
    public decimal Amount { get; private set; }
    public long SubtransactionTypeId { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public DateTime ApplicationDate { get; private set; }

    public ClientOperation ClientOperation { get; private set; } = null!;

    private TrustOperation()
    {
    }

    public static Result<TrustOperation> Create(
        long clientOperationId,
        long trustId,
        decimal amount,
        long subtransactionTypeId,
        int portfolioId,
        DateTime registrationDate,
        DateTime processDate,
        DateTime applicationDate
    )
    {
        var trustOperation = new TrustOperation
        {
            TrustOperationId = default,
            ClientOperationId = clientOperationId,
            TrustId = trustId,
            Amount = amount,
            SubtransactionTypeId = subtransactionTypeId,
            PortfolioId = portfolioId,
            RegistrationDate = registrationDate,
            ProcessDate = processDate,
            ApplicationDate = applicationDate
        };

        return Result.Success(trustOperation);
    }

    public void UpdateDetails(
        long newClientOperationId,
        long newTrustId,
        decimal newAmount,
        long newSubtransactionTypeId,
        int newPortfolioId,
        DateTime newRegistrationDate,
        DateTime newProcessDate,
        DateTime newApplicationDate
    )
    {
        ClientOperationId = newClientOperationId;
        TrustId = newTrustId;
        Amount = newAmount;
        SubtransactionTypeId = newSubtransactionTypeId;
        PortfolioId = newPortfolioId;
        RegistrationDate = newRegistrationDate;
        ProcessDate = newProcessDate;
        ApplicationDate = newApplicationDate;
    }
}