using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Data;
using Products.Domain.Objectives;
using Products.Integrations.Objectives;
using Products.Integrations.Objectives.CreateObjective;

namespace Products.Application.Objectives.CreateObjective;

internal sealed class CreateObjectiveCommandHandler(
    IObjectiveRepository objectiveRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateObjectiveCommand, ObjectiveResponse>
{
    public async Task<Result<ObjectiveResponse>> Handle(CreateObjectiveCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


        var result = Objective.Create(
            request.ObjectiveTypeId,
            request.AffiliateId,
            request.AlternativeId,
            request.Name,
            request.Status,
            request.CreationDate
        );

        if (result.IsFailure) return Result.Failure<ObjectiveResponse>(result.Error);

        var objective = result.Value;

        objectiveRepository.Insert(objective);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new ObjectiveResponse(
            objective.ObjectiveId,
            objective.ObjectiveTypeId,
            objective.AffiliateId,
            objective.AlternativeId,
            objective.Name,
            objective.Status,
            objective.CreationDate
        );
    }
}