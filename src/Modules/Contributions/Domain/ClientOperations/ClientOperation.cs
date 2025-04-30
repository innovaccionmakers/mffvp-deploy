using Common.SharedKernel.Domain;

namespace Contributions.Domain.ClientOperations;
public sealed class ClientOperation : Entity
{
    public Guid ClientOperationId { get; private set; }
    public DateTime Date { get; private set; }
    public int AffiliateId { get; private set; }
    public int ObjectiveId { get; private set; }
    public int PortfolioId { get; private set; }
    public int TransactionTypeId { get; private set; }
    public int SubTransactionTypeId { get; private set; }
    public decimal Amount { get; private set; }

    private ClientOperation() { }

    public static Result<ClientOperation> Create(
        DateTime date, int affiliateid, int objectiveid, int portfolioid, int transactiontypeid, int subtransactiontypeid, decimal amount
    )
    {
        var clientoperation = new ClientOperation
        {
                ClientOperationId = Guid.NewGuid(),
                Date = date,
                AffiliateId = affiliateid,
                ObjectiveId = objectiveid,
                PortfolioId = portfolioid,
                TransactionTypeId = transactiontypeid,
                SubTransactionTypeId = subtransactiontypeid,
                Amount = amount,
        };
        clientoperation.Raise(new ClientOperationCreatedDomainEvent(clientoperation.ClientOperationId));
        return Result.Success(clientoperation);
    }

    public void UpdateDetails(
        DateTime newDate, int newAffiliateId, int newObjectiveId, int newPortfolioId, int newTransactionTypeId, int newSubTransactionTypeId, decimal newAmount
    )
    {
        Date = newDate;
        AffiliateId = newAffiliateId;
        ObjectiveId = newObjectiveId;
        PortfolioId = newPortfolioId;
        TransactionTypeId = newTransactionTypeId;
        SubTransactionTypeId = newSubTransactionTypeId;
        Amount = newAmount;
    }
}