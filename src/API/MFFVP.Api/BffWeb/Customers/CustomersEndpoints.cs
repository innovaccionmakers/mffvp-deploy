using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Customers;

namespace MFFVP.Api.BffWeb.Customers;

public sealed class CustomersEndpoints : IEndpoint
{
    private readonly ICustomersService _customersService;

    public CustomersEndpoints(
        ICustomersService customersService
    )
    {
        _customersService = customersService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var customersEndpoints = new Customers.CustomersEndpoints(_customersService);
        customersEndpoints.MapEndpoint(app);
    }
}
