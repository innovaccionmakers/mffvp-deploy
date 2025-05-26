using MFFVP.Api.Application.Operations;
using MFFVP.Api.Infrastructure.Operations;

namespace MFFVP.Api.BffWeb.Operations;

public static class OperationsServiceRegistration
{
    public static IServiceCollection AddBffOperationsServices(this IServiceCollection services)
    {
        services.AddSingleton<IOperationsService, OperationsService>();
        return services;
    }
}