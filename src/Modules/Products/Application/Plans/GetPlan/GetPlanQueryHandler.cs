using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Plans;
using Products.Integrations.Plans;
using Products.Integrations.Plans.GetPlan;

namespace Products.Application.Plans.GetPlan;

internal sealed class GetPlanQueryHandler(
    IPlanRepository planRepository)
    : IQueryHandler<GetPlanQuery, PlanResponse>
{
    public async Task<Result<PlanResponse>> Handle(GetPlanQuery request, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetAsync(request.PlanId, cancellationToken);
        if (plan is null) return Result.Failure<PlanResponse>(PlanErrors.NotFound(request.PlanId));
        var response = new PlanResponse(
            plan.PlanId,
            plan.Name,
            plan.Description
        );
        return response;
    }
}