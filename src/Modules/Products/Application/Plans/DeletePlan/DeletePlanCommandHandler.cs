using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Data;
using Products.Domain.Plans;
using Products.Integrations.Plans.DeletePlan;

namespace Products.Application.Plans.DeletePlan;

internal sealed class DeletePlanCommandHandler(
    IPlanRepository planRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeletePlanCommand>
{
    public async Task<Result> Handle(DeletePlanCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var plan = await planRepository.GetAsync(request.PlanId, cancellationToken);
        if (plan is null) return Result.Failure(PlanErrors.NotFound(request.PlanId));

        planRepository.Delete(plan);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}