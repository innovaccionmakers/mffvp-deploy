using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.TrustOperations;
using Contributions.Integrations.TrustOperations.DeleteTrustOperation;
using Contributions.Application.Abstractions.Data;

namespace Contributions.Application.TrustOperations.DeleteTrustOperation;

internal sealed class DeleteTrustOperationCommandHandler(
    ITrustOperationRepository trustoperationRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTrustOperationCommand>
{
    public async Task<Result> Handle(DeleteTrustOperationCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var trustoperation = await trustoperationRepository.GetAsync(request.TrustOperationId, cancellationToken);
        if (trustoperation is null)
        {
            return Result.Failure(TrustOperationErrors.NotFound(request.TrustOperationId));
        }
        
        trustoperationRepository.Delete(trustoperation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}