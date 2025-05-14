using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Objectives;
using Products.Integrations.Objectives.UpdateObjective;
using Products.Integrations.Objectives;
using Products.Application.Abstractions.Data;

namespace Products.Application.Objectives;
internal sealed class UpdateObjectiveCommandHandler(
    IObjectiveRepository objectiveRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateObjectiveCommand, ObjectiveResponse>
{
    public async Task<Result<ObjectiveResponse>> Handle(UpdateObjectiveCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await objectiveRepository.GetAsync(request.ObjectiveId, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<ObjectiveResponse>(ObjectiveErrors.NotFound(request.ObjectiveId));
        }

        entity.UpdateDetails(
            request.NewObjectiveTypeId, 
            request.NewAffiliateId, 
            request.NewAlternativeId, 
            request.NewName, 
            request.NewStatus, 
            request.NewCreationDate
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ObjectiveResponse(entity.ObjectiveId, entity.ObjectiveTypeId, entity.AffiliateId, entity.AlternativeId, entity.Name, entity.Status, entity.CreationDate);
    }
}