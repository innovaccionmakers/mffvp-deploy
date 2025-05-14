using MFFVP.Api.Application.Associate;
using MFFVP.Api.Infrastructure.Associate;

namespace MFFVP.Api.BffWeb.Associate;

public static class AssociateServiceRegistration
{
    public static IServiceCollection AddBffActivatesServices(this IServiceCollection services)
    {
        services.AddSingleton<IActivatesService, ActivatesService>();
        return services;
    }
}