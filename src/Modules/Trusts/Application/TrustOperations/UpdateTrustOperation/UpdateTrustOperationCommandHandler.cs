using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.TrustOperations;
using Trusts.Integrations.TrustOperations;
using Trusts.Integrations.TrustOperations.UpdateTrustOperation;

namespace Trusts.Application.TrustOperations.UpdateTrustOperation;

internal sealed class UpdateTrustOperationCommandHandler(
    ITrustOperationRepository trustoperationRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTrustOperationCommand, TrustOperationResponse>
{
    public async Task<Result<TrustOperationResponse>> Handle(UpdateTrustOperationCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await trustoperationRepository.GetAsync(request.TrustOperationId, cancellationToken);
        if (entity is null)
            return Result.Failure<TrustOperationResponse>(TrustOperationErrors.NotFound(request.TrustOperationId));

        entity.UpdateDetails(
            request.NewCustomerDealId,
            request.NewTrustId,
            request.NewAmount
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TrustOperationResponse(entity.TrustOperationId, entity.CustomerDealId, entity.TrustId,
            entity.Amount);
    }
}