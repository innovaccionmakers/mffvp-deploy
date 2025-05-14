using Asp.Versioning;
using Associate.Integrations.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using MFFVP.Api.Application.Associate;
using Microsoft.AspNetCore.Mvc;

namespace MFFVP.Api.BffWeb.Associate.Activates;

public sealed class ActivatesEndpoints
{
    private readonly IActivatesService _activatesService;

    public ActivatesEndpoints(IActivatesService activatesService)
    {
        _activatesService = activatesService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("bffWeb/api/associate/activate")
            .WithTags("BFF Web - Associate")
            .WithOpenApi();

        group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _activatesService.GetActivatesAsync(sender);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .Produces<IReadOnlyCollection<ActivateResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("Create", async ([FromBody] CreateActivateCommand request, ISender sender) =>
            {
                var result = await _activatesService.CreateActivateAsync(request, sender);
                return result.ToApiResult();
            })
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}