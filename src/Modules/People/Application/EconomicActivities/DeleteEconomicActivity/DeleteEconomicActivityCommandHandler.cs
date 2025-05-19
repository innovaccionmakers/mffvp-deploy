using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.EconomicActivities;
using People.Integrations.EconomicActivities.DeleteEconomicActivity;
using People.Application.Abstractions.Data;

namespace People.Application.EconomicActivities.DeleteEconomicActivity;

internal sealed class DeleteEconomicActivityCommandHandler(
    IEconomicActivityRepository economicactivityRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteEconomicActivityCommand>
{
    public async Task<Result> Handle(DeleteEconomicActivityCommand request, CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var economicactivity = await economicactivityRepository.GetAsync(request.EconomicActivityId, cancellationToken);
        if (economicactivity is null)
            return Result.Failure(EconomicActivityErrors.NotFound(request.EconomicActivityId));

        economicactivityRepository.Delete(economicactivity);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}