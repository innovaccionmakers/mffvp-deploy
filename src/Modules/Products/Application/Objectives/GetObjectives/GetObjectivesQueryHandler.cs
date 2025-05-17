using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Objectives;
using Products.Integrations.Objectives;
using Products.Integrations.Objectives.GetObjectives;

namespace Products.Application.Objectives.GetObjectives;

internal sealed class GetObjectivesQueryHandler(
    IObjectiveRepository objectiveRepository)
    : IQueryHandler<GetObjectivesQuery, IReadOnlyCollection<ObjectiveResponse>>
{
    public async Task<Result<IReadOnlyCollection<ObjectiveResponse>>> Handle(GetObjectivesQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await objectiveRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new ObjectiveResponse(
                e.ObjectiveId,
                e.ObjectiveTypeId,
                e.AffiliateId,
                e.AlternativeId,
                e.Name,
                e.Status,
                e.CreationDate))
            .ToList();

        return Result.Success<IReadOnlyCollection<ObjectiveResponse>>(response);
    }
}