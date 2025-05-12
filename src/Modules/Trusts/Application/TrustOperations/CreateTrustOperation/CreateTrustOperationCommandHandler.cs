using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.CustomerDeals;
using Trusts.Domain.TrustOperations;
using Trusts.Domain.Trusts;
using Trusts.Integrations.TrustOperations;
using Trusts.Integrations.TrustOperations.CreateTrustOperation;

namespace Trusts.Application.TrustOperations.CreateTrustOperation;

internal sealed class CreateTrustOperationCommandHandler(
    ICustomerDealRepository customerdealRepository,
    ITrustRepository trustRepository,
    ITrustOperationRepository trustoperationRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateTrustOperationCommand, TrustOperationResponse>
{
    public async Task<Result<TrustOperationResponse>> Handle(CreateTrustOperationCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var customerdeal = await customerdealRepository.GetAsync(request.CustomerDealId, cancellationToken);
        var trust = await trustRepository.GetAsync(request.TrustId, cancellationToken);

        if (customerdeal is null)
            return Result.Failure<TrustOperationResponse>(CustomerDealErrors.NotFound(request.CustomerDealId));
        if (trust is null)
            return Result.Failure<TrustOperationResponse>(TrustErrors.NotFound(request.TrustId));


        var result = TrustOperation.Create(
            request.Amount,
            customerdeal,
            trust
        );

        if (result.IsFailure) return Result.Failure<TrustOperationResponse>(result.Error);

        var trustoperation = result.Value;

        trustoperationRepository.Insert(trustoperation);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TrustOperationResponse(
            trustoperation.TrustOperationId,
            trustoperation.CustomerDealId,
            trustoperation.TrustId,
            trustoperation.Amount
        );
    }
}