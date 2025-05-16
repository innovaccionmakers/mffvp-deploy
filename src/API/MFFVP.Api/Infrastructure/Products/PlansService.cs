
using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Plans;
using Products.Integrations.Plans.CreatePlan;
using Products.Integrations.Plans.DeletePlan;
using Products.Integrations.Plans.GetPlan;
using Products.Integrations.Plans.GetPlans;
using Products.Integrations.Plans.UpdatePlan;

using MFFVP.Api.Application.Products;

namespace MFFVP.Api.Infrastructure.Products
{
    public sealed class PlansService : IPlansService
    {
        public async Task<Result<IReadOnlyCollection<PlanResponse>>> GetPlansAsync(ISender sender)
        {
            return await sender.Send(new GetPlansQuery());
        }

        public async Task<Result<PlanResponse>> GetPlanAsync(long planId, ISender sender)
        {
            return await sender.Send(new GetPlanQuery(planId));
        }

        public async Task<Result> CreatePlanAsync(CreatePlanCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdatePlanAsync(UpdatePlanCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeletePlanAsync(long planId, ISender sender)
        {
            return await sender.Send(new DeletePlanCommand(planId));
        }
    }
}