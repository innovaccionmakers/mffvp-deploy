namespace Operations.Domain.TemporaryClientOperations;

using Common.SharedKernel.Domain;
using Operations.Domain.SubtransactionTypes;
using Operations.Domain.TemporaryAuxiliaryInformations;

public sealed class TemporaryClientOperation : Entity
{
    public long TemporaryClientOperationId { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public int AffiliateId { get; private set; }
    public int ObjectiveId { get; private set; }
    public int PortfolioId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public long SubtransactionTypeId { get; private set; }
    public DateTime ApplicationDate { get; private set; }
    public bool Processed { get; private set; }

    public SubtransactionType SubtransactionType { get; private set; } = null!;
    public TemporaryAuxiliaryInformation TemporaryAuxiliaryInformation { get; private set; } = null!;

    private TemporaryClientOperation()
    {
    }

    public static Result<TemporaryClientOperation> Create(
        DateTime registrationDate,
        int affiliateId,
        int objectiveId,
        int portfolioId,
        decimal amount,
        DateTime processDate,
        long subtransactionTypeId,
        DateTime applicationDate
    )
    {
        var clientOperation = new TemporaryClientOperation
        {
            TemporaryClientOperationId = default,
            RegistrationDate = registrationDate,
            AffiliateId = affiliateId,
            ObjectiveId = objectiveId,
            PortfolioId = portfolioId,
            Amount = amount,
            ProcessDate = processDate,
            SubtransactionTypeId = subtransactionTypeId,
            ApplicationDate = applicationDate,
            Processed = false
        };

        clientOperation.Raise(new TemporaryClientOperationCreatedDomainEvent(clientOperation.TemporaryClientOperationId));
        return Result.Success(clientOperation);
    }

    public void UpdateDetails(
        DateTime newRegistrationDate,
        int newAffiliateId,
        int newObjectiveId,
        int newPortfolioId,
        decimal newAmount,
        DateTime newProcessDate,
        long newSubtransactionTypeId,
        DateTime newApplicationDate
    )
    {
        RegistrationDate = newRegistrationDate;
        AffiliateId = newAffiliateId;
        ObjectiveId = newObjectiveId;
        PortfolioId = newPortfolioId;
        Amount = newAmount;
        ProcessDate = newProcessDate;
        SubtransactionTypeId = newSubtransactionTypeId;
        ApplicationDate = newApplicationDate;
    }
    
    public void MarkAsProcessed()
    {
        Processed = true;
    }
}
