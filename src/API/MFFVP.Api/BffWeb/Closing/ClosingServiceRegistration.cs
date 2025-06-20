using MFFVP.Api.Application.Closing;
using MFFVP.Api.Infrastructure.Closing;

namespace MFFVP.Api.BffWeb.Closing
{
    public static class ClosingServiceRegistration
    {
        public static IServiceCollection AddBffClosingServices(this IServiceCollection services)
        {
            services.AddSingleton<IProfitLossService, ProfitLossService>();
            return services;
        }
    }
}