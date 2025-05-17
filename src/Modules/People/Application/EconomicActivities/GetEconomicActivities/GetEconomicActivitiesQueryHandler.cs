using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using People.Domain.EconomicActivities;
using People.Integrations.EconomicActivities.GetEconomicActivities;
using People.Integrations.EconomicActivities;
using System.Collections.Generic;
using System.Linq;

namespace People.Application.EconomicActivities.GetEconomicActivities;

internal sealed class GetEconomicActivitiesQueryHandler(
    IEconomicActivityRepository economicactivityRepository)
    : IQueryHandler<GetEconomicActivitiesQuery, IReadOnlyCollection<EconomicActivityResponse>>
{
    public async Task<Result<IReadOnlyCollection<EconomicActivityResponse>>> Handle(GetEconomicActivitiesQuery request,
        CancellationToken cancellationToken)
    {
        var entities = await economicactivityRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new EconomicActivityResponse(
                e.EconomicActivityId,
                e.Description,
                e.CiiuCode,
                e.DivisionCode,
                e.DivisionName,
                e.GroupName,
                e.ClassCode,
                e.StandardCode))
            .ToList();

        return Result.Success<IReadOnlyCollection<EconomicActivityResponse>>(response);
    }
}