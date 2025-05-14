using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.EconomicActivities;
using People.Integrations.EconomicActivities.GetEconomicActivity;
using People.Integrations.EconomicActivities;

namespace People.Application.EconomicActivities.GetEconomicActivity;

internal sealed class GetEconomicActivityQueryHandler(
    IEconomicActivityRepository economicactivityRepository)
    : IQueryHandler<GetEconomicActivityQuery, EconomicActivityResponse>
{
    public async Task<Result<EconomicActivityResponse>> Handle(GetEconomicActivityQuery request, CancellationToken cancellationToken)
    {
        var economicactivity = await economicactivityRepository.GetAsync(request.EconomicActivityId, cancellationToken);
        if (economicactivity is null)
        {
            return Result.Failure<EconomicActivityResponse>(EconomicActivityErrors.NotFound(request.EconomicActivityId));
        }
        var response = new EconomicActivityResponse(
            economicactivity.EconomicActivityId,
            economicactivity.Description,
            economicactivity.CiiuCode,
            economicactivity.DivisionCode,
            economicactivity.DivisionName,
            economicactivity.GroupName,
            economicactivity.ClassCode,
            economicactivity.StandardCode
        );
        return response;
    }
}