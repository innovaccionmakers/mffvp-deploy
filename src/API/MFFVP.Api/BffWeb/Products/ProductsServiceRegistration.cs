using MFFVP.Api.Application.Products;
using MFFVP.Api.Infrastructure.Products;

namespace MFFVP.Api.BffWeb.Products
{
    public static class ProductsServiceRegistration
    {
        public static IServiceCollection AddBffProductsServices(this IServiceCollection services)
        {
            services.AddSingleton<IObjectivesService, ObjectivesService>();
            services.AddSingleton<IPortfoliosService, PortfoliosService>();
            return services;
        }
    }
}