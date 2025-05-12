using Common.SharedKernel.Presentation.Results;
using MediatR;
using MFFVP.Api.Application.Trusts;
using Microsoft.AspNetCore.Mvc;
using Trusts.Integrations.CustomerDeals;
using Trusts.Integrations.CustomerDeals.CreateCustomerDeal;
using Trusts.Integrations.CustomerDeals.UpdateCustomerDeal;

namespace MFFVP.Api.BffWeb.Trusts.CustomerDeals;

public sealed class CustomerDealsEndpoints
{
    private readonly ICustomerDealsService _customerdealsService;

    public CustomerDealsEndpoints(ICustomerDealsService customerdealsService)
    {
        _customerdealsService = customerdealsService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("bffWeb/api/contributions/customerdeals")
            .WithTags("BFF Web - CustomerDeals")
            .WithOpenApi();

        group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _customerdealsService.GetCustomerDealsAsync(sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<IReadOnlyCollection<CustomerDealResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("GetById/{id}", async (Guid id, ISender sender) =>
            {
                var result = await _customerdealsService.GetCustomerDealAsync(id, sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<CustomerDealResponse>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("Create", async ([FromBody] CreateCustomerDealCommand request, ISender sender) =>
            {
                var result = await _customerdealsService.CreateCustomerDealAsync(request, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("Update", async ([FromBody] UpdateCustomerDealCommand command, ISender sender) =>
            {
                var result = await _customerdealsService.UpdateCustomerDealAsync(command, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapDelete("Delete/{id}", async (Guid id, ISender sender) =>
            {
                var result = await _customerdealsService.DeleteCustomerDealAsync(id, sender);
                return result.Match(() => Results.NoContent(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}