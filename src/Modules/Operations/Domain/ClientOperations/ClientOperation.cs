using Common.SharedKernel.Domain;
using Operations.Domain.AuxiliaryInformations;
using Operations.Domain.TrustWithdrawals;
using Operations.Domain.SubtransactionTypes;

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
    public DateTime CausationDate { get; private set; }
    public long SubtransactionTypeId { get; private set; }

    public SubtransactionType SubtransactionType { get; private set; } = null!;
    private readonly List<TrustWithdrawalOperation> _trustWithdrawals = new();
    public IReadOnlyCollection<TrustWithdrawalOperation> TrustWithdrawals => _trustWithdrawals;

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
        DateTime causationDate,
        long subtransactionTypeId
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
            CausationDate = causationDate,
            SubtransactionTypeId = subtransactionTypeId
        };

        clientOperation.Raise(new ClientOperationCreatedDomainEvent(clientOperation.ClientOperationId));
        return Result.Success(clientOperation);
    }

    public void UpdateDetails(
        DateTime newRegistrationDate,
        int newAffiliateId,
        int newObjectiveId,
        int newPortfolioId,
        decimal newAmount,
        DateTime newProcessDate,
        DateTime newCausationDate,
        long newSubtransactionTypeId
    )
    {
        RegistrationDate = newRegistrationDate;
        AffiliateId = newAffiliateId;
        ObjectiveId = newObjectiveId;
        PortfolioId = newPortfolioId;
        Amount = newAmount;
        ProcessDate = newProcessDate;
        CausationDate = newCausationDate;
        SubtransactionTypeId = newSubtransactionTypeId;
    }
}