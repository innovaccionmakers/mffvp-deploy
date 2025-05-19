using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Plans;
using Products.Integrations.Plans;
using Products.Integrations.Plans.GetPlans;

namespace Products.Application.Plans.GetPlans;

internal sealed class GetPlansQueryHandler(
    IPlanRepository planRepository)
    : IQueryHandler<GetPlansQuery, IReadOnlyCollection<PlanResponse>>
{
    public async Task<Result<IReadOnlyCollection<PlanResponse>>> Handle(GetPlansQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await planRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new PlanResponse(
                e.PlanId,
                e.Name,
                e.Description))
            .ToList();

        return Result.Success<IReadOnlyCollection<PlanResponse>>(response);
    }
}