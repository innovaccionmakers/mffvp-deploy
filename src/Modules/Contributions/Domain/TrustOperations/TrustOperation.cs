using Common.SharedKernel.Domain;
using Contributions.Domain.ClientOperations;
using Contributions.Domain.Trusts;

namespace Contributions.Domain.TrustOperations;
public sealed class TrustOperation : Entity
{
    public Guid TrustOperationId { get; private set; }
    public Guid ClientOperationId { get; private set; }
    public Guid TrustId { get; private set; }
    public decimal Amount { get; private set; }

    private TrustOperation() { }

    public static Result<TrustOperation> Create(
        decimal amount, ClientOperation clientoperation, Trust trust
    )
    {
        var trustoperation = new TrustOperation
        {
                TrustOperationId = Guid.NewGuid(),
                ClientOperationId = clientoperation.ClientOperationId,
                TrustId = trust.TrustId,
                Amount = amount,
        };
        trustoperation.Raise(new TrustOperationCreatedDomainEvent(trustoperation.TrustOperationId));
        return Result.Success(trustoperation);
    }

    public void UpdateDetails(
        Guid newClientOperationId, Guid newTrustId, decimal newAmount
    )
    {
        ClientOperationId = newClientOperationId;
        TrustId = newTrustId;
        Amount = newAmount;
    }
}