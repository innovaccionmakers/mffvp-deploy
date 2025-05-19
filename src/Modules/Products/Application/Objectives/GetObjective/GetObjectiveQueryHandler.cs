using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Objectives;
using Products.Integrations.Objectives;
using Products.Integrations.Objectives.GetObjective;

namespace Products.Application.Objectives.GetObjective;

internal sealed class GetObjectiveQueryHandler(
    IObjectiveRepository objectiveRepository)
    : IQueryHandler<GetObjectiveQuery, ObjectiveResponse>
{
    public async Task<Result<ObjectiveResponse>> Handle(GetObjectiveQuery request, CancellationToken cancellationToken)
    {
        var objective = await objectiveRepository.GetAsync(request.ObjectiveId, cancellationToken);
        if (objective is null) return Result.Failure<ObjectiveResponse>(ObjectiveErrors.NotFound(request.ObjectiveId));
        var response = new ObjectiveResponse(
            objective.ObjectiveId,
            objective.ObjectiveTypeId,
            objective.AffiliateId,
            objective.AlternativeId,
            objective.Name,
            objective.Status,
            objective.CreationDate
        );
        return response;
    }
}