using MFFVP.Api.Application.Contributions;
using MFFVP.Api.Infrastructure.Contributions;

namespace MFFVP.Api.BffWeb.Contributions
{
    public static class ContributionsServiceRegistration
    {
        public static IServiceCollection AddBffContributionsServices(this IServiceCollection services)
        {
            services.AddSingleton<ITrustsService, TrustsService>();
            services.AddSingleton<IClientOperationsService, ClientOperationsService>();
            services.AddSingleton<ITrustOperationsService, TrustOperationsService>();
            return services;
        }
    }
}