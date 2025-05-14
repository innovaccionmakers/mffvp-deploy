using MFFVP.Api.Application.Trusts;
using MFFVP.Api.Infrastructure.Trusts;

namespace MFFVP.Api.BffWeb.Trusts
{
    public static class TrustsServiceRegistration
    {
        public static IServiceCollection AddBffTrustsServices(this IServiceCollection services)
        {
            services.AddSingleton<ITrustsService, TrustsService>();
            services.AddSingleton<ITrustHistoriesService, TrustHistoriesService>();
            return services;
        }
    }
}