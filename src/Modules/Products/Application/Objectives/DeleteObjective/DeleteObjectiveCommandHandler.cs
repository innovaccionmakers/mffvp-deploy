using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Objectives;
using Products.Integrations.Objectives.DeleteObjective;
using Products.Application.Abstractions.Data;

namespace Products.Application.Objectives.DeleteObjective;

internal sealed class DeleteObjectiveCommandHandler(
    IObjectiveRepository objectiveRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteObjectiveCommand>
{
    public async Task<Result> Handle(DeleteObjectiveCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var objective = await objectiveRepository.GetAsync(request.ObjectiveId, cancellationToken);
        if (objective is null)
        {
            return Result.Failure(ObjectiveErrors.NotFound(request.ObjectiveId));
        }
        
        objectiveRepository.Delete(objective);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}