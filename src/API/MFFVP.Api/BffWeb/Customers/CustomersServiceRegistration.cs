using MFFVP.Api.Application.Customers;
using MFFVP.Api.Infrastructure.Customers;

namespace MFFVP.Api.BffWeb.Customers;

public static class CustomersServiceRegistration
{
    public static IServiceCollection AddBffCustomersServices(this IServiceCollection services)
    {
        services.AddSingleton<ICustomersService, CustomersService>();
        return services;
    }
}
