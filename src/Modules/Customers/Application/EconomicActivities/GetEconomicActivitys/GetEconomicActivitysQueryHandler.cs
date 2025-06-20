using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.EconomicActivities;
using Customers.Integrations.EconomicActivities.GetEconomicActivitys;
using Customers.Integrations.EconomicActivities;
using System.Collections.Generic;
using System.Linq;

namespace Customers.Application.EconomicActivities.GetEconomicActivities;

internal sealed class GetEconomicActivitysQueryHandler(
    IEconomicActivityRepository economicactivityRepository)
    : IQueryHandler<GetEconomicActivitysQuery, IReadOnlyCollection<EconomicActivityResponse>>
{
    public async Task<Result<IReadOnlyCollection<EconomicActivityResponse>>> Handle(GetEconomicActivitysQuery request, CancellationToken cancellationToken)
    {
        var entities = await economicactivityRepository.GetAllAsync(cancellationToken);
        
        var response = entities
            .Select(e => new EconomicActivityResponse(
                e.EconomicActivityId,
                e.GroupCode,
                e.Description,
                e.CiiuCode,
                e.DivisionCode,
                e.DivisionName,
                e.GroupName,
                e.ClassCode,
                e.HomologatedCode))
            .ToList();

        return Result.Success<IReadOnlyCollection<EconomicActivityResponse>>(response);
    }
}