using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Trusts.Application.Abstractions.Data;
using Trusts.Domain.TrustOperations;
using Trusts.Integrations.TrustOperations.DeleteTrustOperation;

namespace Trusts.Application.TrustOperations.DeleteTrustOperation;

internal sealed class DeleteTrustOperationCommandHandler(
    ITrustOperationRepository trustoperationRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteTrustOperationCommand>
{
    public async Task<Result> Handle(DeleteTrustOperationCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var trustoperation = await trustoperationRepository.GetAsync(request.TrustOperationId, cancellationToken);
        if (trustoperation is null) return Result.Failure(TrustOperationErrors.NotFound(request.TrustOperationId));

        trustoperationRepository.Delete(trustoperation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}