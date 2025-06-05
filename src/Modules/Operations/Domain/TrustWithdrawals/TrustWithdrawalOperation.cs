using Common.SharedKernel.Domain;
using Operations.Domain.ClientOperations;

namespace Operations.Domain.TrustWithdrawals;

public sealed class TrustWithdrawalOperation : Entity
{
    public long TrustWithdrawalOperationId { get; private set; }
    public long ClientOperationId { get; private set; }
    public long TrustId { get; private set; }
    public decimal Amount { get; private set; }

    public ClientOperation ClientOperation { get; private set; } = null!;

    private TrustWithdrawalOperation()
    {
    }

    public static Result<TrustWithdrawalOperation> Create(
        long clientOperationId,
        long trustId,
        decimal amount
    )
    {
        var trustWithdrawal = new TrustWithdrawalOperation
        {
            TrustWithdrawalOperationId = default,
            ClientOperationId = clientOperationId,
            TrustId = trustId,
            Amount = amount
        };

        return Result.Success(trustWithdrawal);
    }

    public void UpdateDetails(
        long newClientOperationId,
        long newTrustId,
        decimal newAmount
    )
    {
        ClientOperationId = newClientOperationId;
        TrustId = newTrustId;
        Amount = newAmount;
    }
}