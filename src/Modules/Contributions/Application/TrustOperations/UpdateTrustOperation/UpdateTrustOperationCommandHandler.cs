using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.TrustOperations;
using Contributions.Integrations.TrustOperations.UpdateTrustOperation;
using Contributions.Integrations.TrustOperations;
using Contributions.Application.Abstractions.Data;

namespace Contributions.Application.TrustOperations;
internal sealed class UpdateTrustOperationCommandHandler(
    ITrustOperationRepository trustoperationRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateTrustOperationCommand, TrustOperationResponse>
{
    public async Task<Result<TrustOperationResponse>> Handle(UpdateTrustOperationCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await trustoperationRepository.GetAsync(request.TrustOperationId, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<TrustOperationResponse>(TrustOperationErrors.NotFound(request.TrustOperationId));
        }

        entity.UpdateDetails(
            request.NewClientOperationId, 
            request.NewTrustId, 
            request.NewAmount
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TrustOperationResponse(entity.TrustOperationId, entity.ClientOperationId, entity.TrustId, entity.Amount);
    }
}