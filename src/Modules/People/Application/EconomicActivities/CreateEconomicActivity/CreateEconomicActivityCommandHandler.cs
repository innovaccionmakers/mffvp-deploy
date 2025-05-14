using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.EconomicActivities;
using People.Integrations.EconomicActivities.CreateEconomicActivity;
using People.Integrations.EconomicActivities;
using People.Application.Abstractions.Data;

namespace People.Application.EconomicActivities.CreateEconomicActivity

{
    internal sealed class CreateEconomicActivityCommandHandler(
        IEconomicActivityRepository economicactivityRepository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<CreateEconomicActivityCommand, EconomicActivityResponse>
    {
        public async Task<Result<EconomicActivityResponse>> Handle(CreateEconomicActivityCommand request, CancellationToken cancellationToken)
        {
            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


            var result = EconomicActivity.Create(
                request.EconomicActivityId,
                request.Description,
                request.CiiuCode,
                request.DivisionCode,
                request.DivisionName,
                request.GroupName,
                request.ClassCode,
                request.StandardCode
            );

            if (result.IsFailure)
            {
                return Result.Failure<EconomicActivityResponse>(result.Error);
            }

            var economicactivity = result.Value;
            
            economicactivityRepository.Insert(economicactivity);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new EconomicActivityResponse(
                economicactivity.EconomicActivityId,
                economicactivity.Description,
                economicactivity.CiiuCode,
                economicactivity.DivisionCode,
                economicactivity.DivisionName,
                economicactivity.GroupName,
                economicactivity.ClassCode,
                economicactivity.StandardCode
            );
        }
    }
}