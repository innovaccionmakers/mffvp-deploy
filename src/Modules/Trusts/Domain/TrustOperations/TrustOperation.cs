using Common.SharedKernel.Domain;
using Trusts.Domain.CustomerDeals;
using Trusts.Domain.Trusts;

namespace Trusts.Domain.TrustOperations;

public sealed class TrustOperation : Entity
{
    private TrustOperation()
    {
    }

    public Guid TrustOperationId { get; private set; }
    public Guid CustomerDealId { get; private set; }
    public Guid TrustId { get; private set; }
    public decimal Amount { get; private set; }

    public static Result<TrustOperation> Create(
        decimal amount, CustomerDeal customerDeal, Trust trust
    )
    {
        var trustOperation = new TrustOperation
        {
            TrustOperationId = Guid.NewGuid(),
            CustomerDealId = customerDeal.CustomerDealId,
            TrustId = trust.TrustId,
            Amount = amount
        };
        trustOperation.Raise(new TrustOperationCreatedDomainEvent(trustOperation.TrustOperationId));
        return Result.Success(trustOperation);
    }

    public void UpdateDetails(
        Guid newCustomerDealId, Guid newTrustId, decimal newAmount
    )
    {
        CustomerDealId = newCustomerDealId;
        TrustId = newTrustId;
        Amount = newAmount;
    }
}