using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.EconomicActivities;
using People.Integrations.EconomicActivities.UpdateEconomicActivity;
using People.Integrations.EconomicActivities;
using People.Application.Abstractions.Data;

namespace People.Application.EconomicActivities;

internal sealed class UpdateEconomicActivityCommandHandler(
    IEconomicActivityRepository economicactivityRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateEconomicActivityCommand, EconomicActivityResponse>
{
    public async Task<Result<EconomicActivityResponse>> Handle(UpdateEconomicActivityCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await economicactivityRepository.GetAsync(request.EconomicActivityId, cancellationToken);
        if (entity is null)
            return Result.Failure<EconomicActivityResponse>(
                EconomicActivityErrors.NotFound(request.EconomicActivityId));

        entity.UpdateDetails(
            request.NewEconomicActivityId,
            request.NewDescription,
            request.NewCiiuCode,
            request.NewDivisionCode,
            request.NewDivisionName,
            request.NewGroupName,
            request.NewClassCode,
            request.NewStandardCode
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new EconomicActivityResponse(entity.EconomicActivityId, entity.Description, entity.CiiuCode,
            entity.DivisionCode, entity.DivisionName, entity.GroupName, entity.ClassCode, entity.StandardCode);
    }
}