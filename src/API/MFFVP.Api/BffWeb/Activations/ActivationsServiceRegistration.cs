using MFFVP.Api.Application.Activations;
using MFFVP.Api.Infrastructure.Activations;

namespace MFFVP.Api.BffWeb.Activations;

public static class ActivationsServiceRegistration
{
    public static IServiceCollection AddBffActivationsServices(this IServiceCollection services)
    {
        services.AddSingleton<IAffiliatesService, AffiliatesService>();
        services.AddSingleton<IMeetsPensionRequirementsService, MeetsPensionRequirementsService>();
        return services;
    }
}