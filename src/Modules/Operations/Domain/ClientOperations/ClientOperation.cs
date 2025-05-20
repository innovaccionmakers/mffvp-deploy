using Common.SharedKernel.Domain;

namespace Operations.Domain.ClientOperations;

public sealed class ClientOperation : Entity
{
    public long ClientOperationId { get; private set; }
    public DateTime Date { get; private set; }
    public int AffiliateId { get; private set; }
    public int ObjectiveId { get; private set; }
    public int PortfolioId { get; private set; }
    public decimal Amount { get; private set; }
    public int SubtransactionTypeId { get; private set; }

    private ClientOperation()
    {
    }

    public static Result<ClientOperation> Create(
        DateTime date, int affiliateId, int objectiveId, int portfolioId, decimal amount, int subtransactionTypeId
    )
    {
        var clientoperation = new ClientOperation
        {
            ClientOperationId = default,

            Date = date,
            AffiliateId = affiliateId,
            ObjectiveId = objectiveId,
            PortfolioId = portfolioId,
            Amount = amount,
            SubtransactionTypeId = subtransactionTypeId
        };

        clientoperation.Raise(new ClientOperationCreatedDomainEvent(clientoperation.ClientOperationId));
        return Result.Success(clientoperation);
    }

    public void UpdateDetails(
        DateTime newDate, int newAffiliateId, int newObjectiveId, int newPortfolioId, decimal newAmount,
        int newSubtransactionTypeId
    )
    {
        Date = newDate;
        AffiliateId = newAffiliateId;
        ObjectiveId = newObjectiveId;
        PortfolioId = newPortfolioId;
        Amount = newAmount;
        SubtransactionTypeId = newSubtransactionTypeId;
    }
}