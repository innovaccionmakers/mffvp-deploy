using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Plans;
using Products.Integrations.Plans.UpdatePlan;
using Products.Integrations.Plans;
using Products.Application.Abstractions.Data;

namespace Products.Application.Plans;
internal sealed class UpdatePlanCommandHandler(
    IPlanRepository planRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdatePlanCommand, PlanResponse>
{
    public async Task<Result<PlanResponse>> Handle(UpdatePlanCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await planRepository.GetAsync(request.PlanId, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<PlanResponse>(PlanErrors.NotFound(request.PlanId));
        }

        entity.UpdateDetails(
            request.NewName, 
            request.NewDescription
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new PlanResponse(entity.PlanId, entity.Name, entity.Description);
    }
}