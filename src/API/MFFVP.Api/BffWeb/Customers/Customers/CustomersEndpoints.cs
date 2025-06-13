using MediatR;
using Customers.Integrations.People;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using MFFVP.Api.Application.Customers;
using Common.SharedKernel.Presentation.Filters;
using Integrations.People.CreatePerson;

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
        var group = app.MapGroup("bffWeb/FVP/Customer")
            .WithTags("BFF Web - Customers")
            .WithOpenApi();

        group.MapGet("GetCustomer", async (ISender sender) =>
        {
            var result = await _customersService.GetPersonsAsync(sender);
            return result.Value;
        })
        .Produces<IReadOnlyCollection<PersonResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("PostCustomer", async ([FromBody] CreatePersonRequestCommand request, ISender sender) =>
        {
            var result = await _customersService.CreatePersonAsync(request, sender);
            return result.ToApiResult(result.Description);
        })
        .AddEndpointFilter<TechnicalValidationFilter<CreatePersonRequestCommand>>()
        .Produces<PersonResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
