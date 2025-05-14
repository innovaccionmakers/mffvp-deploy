using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Plans;
using Products.Integrations.Plans.CreatePlan;
using Products.Integrations.Plans;
using Products.Application.Abstractions.Data;

namespace Products.Application.Plans.CreatePlan

{
    internal sealed class CreatePlanCommandHandler(
        IPlanRepository planRepository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<CreatePlanCommand, PlanResponse>
    {
        public async Task<Result<PlanResponse>> Handle(CreatePlanCommand request, CancellationToken cancellationToken)
        {
            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


            var result = Plan.Create(
                request.Name,
                request.Description
            );

            if (result.IsFailure)
            {
                return Result.Failure<PlanResponse>(result.Error);
            }

            var plan = result.Value;
            
            planRepository.Insert(plan);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new PlanResponse(
                plan.PlanId,
                plan.Name,
                plan.Description
            );
        }
    }
}