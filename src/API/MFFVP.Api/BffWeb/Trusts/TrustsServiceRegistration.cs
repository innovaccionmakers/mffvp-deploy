using MFFVP.Api.Application.Trusts;
using MFFVP.Api.Infrastructure.Trusts;

namespace MFFVP.Api.BffWeb.Trusts;

public static class TrustsServiceRegistration
{
    public static IServiceCollection AddBffContributionsServices(this IServiceCollection services)
    {
        services.AddSingleton<ITrustsService, TrustsService>();
        services.AddSingleton<ICustomerDealsService, CustomerDealsService>();
        services.AddSingleton<ITrustOperationsService, TrustOperationsService>();
        services.AddSingleton<IInputInfosService, InputInfosService>();
        services.AddSingleton<IFullContributionService, FullContributionService>();
        return services;
    }
}