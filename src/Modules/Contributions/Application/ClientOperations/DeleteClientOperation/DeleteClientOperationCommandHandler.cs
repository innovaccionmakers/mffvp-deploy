using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Contributions.Domain.ClientOperations;
using Contributions.Integrations.ClientOperations.DeleteClientOperation;
using Contributions.Application.Abstractions.Data;

namespace Contributions.Application.ClientOperations.DeleteClientOperation;

internal sealed class DeleteClientOperationCommandHandler(
    IClientOperationRepository clientoperationRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteClientOperationCommand>
{
    public async Task<Result> Handle(DeleteClientOperationCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var clientoperation = await clientoperationRepository.GetAsync(request.ClientOperationId, cancellationToken);
        if (clientoperation is null)
        {
            return Result.Failure(ClientOperationErrors.NotFound(request.ClientOperationId));
        }
        
        clientoperationRepository.Delete(clientoperation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}