using Common.SharedKernel.Presentation.Results;
using MediatR;
using MFFVP.Api.Application.Operations;
using Microsoft.AspNetCore.Mvc;
using Operations.Integrations.Contributions.CreateContribution;

namespace MFFVP.Api.BffWeb.Operations.Operations;

public sealed class OperationsEndpoints
{
    private IOperationsService _operationsService;

    public OperationsEndpoints(IOperationsService operationsService)
    {
        _operationsService = operationsService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("bffWeb/FVP/Product/FVP")
            .WithTags("BFF Web - Contributions")
            .WithOpenApi();

        group.MapPost("ContributionTx", async ([FromBody] CreateContributionCommand request, ISender sender) =>
            {
                var result = await _operationsService.CreateContribution(request, sender);
                return result.Match(() => Results.Ok(), ApiResults.Problem);
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}