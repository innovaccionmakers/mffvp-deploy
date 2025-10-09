namespace Operations.Domain.TemporaryClientOperations;

using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Operations.Domain.OperationTypes;
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
    public long OperationTypeId { get; private set; }
    public DateTime ApplicationDate { get; private set; }
    public bool Processed { get; private set; }
    public long? TrustId { get; private set; }
    public long? LinkedClientOperationId { get; private set; }
    public LifecycleStatus Status { get; private set; }
    public decimal? Units { get; private set; }
    public int? CauseId { get; private set; }

    public OperationType OperationType { get; private set; } = null!;
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
        long operationTypeId,
        DateTime applicationDate,
        LifecycleStatus status,
        int? causeId = null,
        long? trustId = null,
        long? linkedClientOperationId = null,
        decimal? units = null
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
            OperationTypeId = operationTypeId,
            ApplicationDate = applicationDate,
            Processed = false,
            Status = status,
            CauseId = causeId,
            TrustId = trustId,
            LinkedClientOperationId = linkedClientOperationId,
            Units = units
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
        long newOperationTypeId,
        DateTime newApplicationDate,
        LifecycleStatus newStatus,
        int? newCauseId = null,
        long? newTrustId = null,
        long? newLinkedClientOperationId = null,
        decimal? newUnits = null
    )
    {
        RegistrationDate = newRegistrationDate;
        AffiliateId = newAffiliateId;
        ObjectiveId = newObjectiveId;
        PortfolioId = newPortfolioId;
        Amount = newAmount;
        ProcessDate = newProcessDate;
        OperationTypeId = newOperationTypeId;
        ApplicationDate = newApplicationDate;
        Status = newStatus;
        CauseId = newCauseId;
        TrustId = newTrustId;
        LinkedClientOperationId = newLinkedClientOperationId;
        Units = newUnits;
    }
    
    public void MarkAsProcessed()
    {
        Processed = true;
    }
}
