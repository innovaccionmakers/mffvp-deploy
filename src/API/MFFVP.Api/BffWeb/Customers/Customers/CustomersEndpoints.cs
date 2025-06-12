using MediatR;
using Customers.Integrations.People.UpdatePerson;
using Customers.Integrations.People;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using MFFVP.Api.Application.Customers;

namespace MFFVP.Api.BffWeb.Customers.Customers;

public sealed class CustomersEndpoints
{
    private readonly ICustomersService _customersService;

    public CustomersEndpoints(ICustomersService customersService)
    {
        _customersService = customersService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("bffWeb/FVP/customers/customers")
            .WithTags("BFF Web - Customers")
            .WithOpenApi();
    }
}
