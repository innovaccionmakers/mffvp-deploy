using Common.SharedKernel.Domain;
using MediatR;

using Products.Integrations.Plans;
using Products.Integrations.Plans.CreatePlan;
using Products.Integrations.Plans.UpdatePlan;

namespace MFFVP.Api.Application.Products
{
    public interface IPlansService
    {
        Task<Result<IReadOnlyCollection<PlanResponse>>> GetPlansAsync(ISender sender);
        Task<Result<PlanResponse>> GetPlanAsync(long planId, ISender sender);
        Task<Result> CreatePlanAsync(CreatePlanCommand request, ISender sender);
        Task<Result> UpdatePlanAsync(UpdatePlanCommand request, ISender sender);
        Task<Result> DeletePlanAsync(long planId, ISender sender);
    }
}