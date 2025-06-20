using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Customers.Domain.EconomicActivities;
using Customers.Integrations.EconomicActivities.GetEconomicActivity;
using Customers.Integrations.EconomicActivities;

namespace Customers.Application.EconomicActivities.GetEconomicActivity;

internal sealed class GetEconomicActivityQueryHandler(
    IEconomicActivityRepository economicactivityRepository)
    : IQueryHandler<GetEconomicActivityQuery, EconomicActivityResponse>
{
    public async Task<Result<EconomicActivityResponse>> Handle(GetEconomicActivityQuery request, CancellationToken cancellationToken)
    {
        var economicactivity = await economicactivityRepository.GetAsync(request.HomologatedCode, cancellationToken);
        if (economicactivity is null)
            return null;
            
        var response = new EconomicActivityResponse(
            economicactivity.EconomicActivityId,
            economicactivity.GroupCode,
            economicactivity.Description,
            economicactivity.CiiuCode,
            economicactivity.DivisionCode,
            economicactivity.DivisionName,
            economicactivity.GroupName,
            economicactivity.ClassCode,
            economicactivity.HomologatedCode
        );
        return response;
    }
}