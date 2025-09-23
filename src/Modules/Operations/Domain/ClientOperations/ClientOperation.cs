using Common.SharedKernel.Domain;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.TrustOperations;
using Operations.Domain.OperationTypes;

namespace Operations.Domain.ClientOperations;

public sealed class ClientOperation : Entity
{
    public long ClientOperationId { get; private set; }
    public DateTime RegistrationDate { get; private set; }
    public int AffiliateId { get; private set; }
    public int ObjectiveId { get; private set; }
    public int PortfolioId { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime ProcessDate { get; private set; }
    public long OperationTypeId { get; private set; }
    public DateTime ApplicationDate { get; private set; }
    public long? TrustId { get; private set; }
    public long? LinkedClientOperationId { get; private set; }
    public int Status { get; private set; }
    public decimal? Units { get; private set; }

    public OperationType OperationType { get; private set; } = null!;
    private readonly List<TrustOperation> _trustOperations = new();
    public IReadOnlyCollection<TrustOperation> TrustOperations => _trustOperations;

    public AuxiliaryInformation AuxiliaryInformation { get; private set; } = null!;

    private ClientOperation()
    {
    }

    public static Result<ClientOperation> Create(
        DateTime registrationDate,
        int affiliateId,
        int objectiveId,
        int portfolioId,
        decimal amount,
        DateTime processDate,
        long operationTypeId,
        DateTime applicationDate,
        int status,
        long? trustId = null,
        long? linkedClientOperationId = null,
        decimal? units = null
    )
    {
        var clientOperation = new ClientOperation
        {
            ClientOperationId = default,
            RegistrationDate = registrationDate,
            AffiliateId = affiliateId,
            ObjectiveId = objectiveId,
            PortfolioId = portfolioId,
            Amount = amount,
            ProcessDate = processDate,
            OperationTypeId = operationTypeId,
            ApplicationDate = applicationDate,
            Status = status,
            TrustId = trustId,
            LinkedClientOperationId = linkedClientOperationId,
            Units = units
        };

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
        int newStatus,
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
        TrustId = newTrustId;
        LinkedClientOperationId = newLinkedClientOperationId;
        Units = newUnits;
    }
}