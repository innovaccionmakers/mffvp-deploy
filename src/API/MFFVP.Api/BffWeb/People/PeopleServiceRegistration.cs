using MFFVP.Api.Application.People;
using MFFVP.Api.Infrastructure.People;

namespace MFFVP.Api.BffWeb.People;

public static class PeopleServiceRegistration
{
    public static IServiceCollection AddBffPeopleServices(this IServiceCollection services)
    {
        services.AddSingleton<IPeopleService, PeopleService>();
        return services;
    }
}