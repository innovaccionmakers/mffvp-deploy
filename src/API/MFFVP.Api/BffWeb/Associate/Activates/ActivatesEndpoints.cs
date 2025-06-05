using Associate.Integrations.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.Activates.UpdateActivate;
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
        var group = app.MapGroup("bffWeb/FVP/associate/activate")
            .WithTags("BFF Web - Associate")
            .WithOpenApi();

        group.MapGet("GetAll", async (ISender sender) =>
            {
                var result = await _activatesService.GetActivatesAsync(sender);
                return result.Value;
            })
            .Produces<IReadOnlyCollection<ActivateResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("Create", async ([FromBody] CreateActivateCommand request, ISender sender) =>
            {
                var result = await _activatesService.CreateActivateAsync(request, sender);
                return result.ToApiResult(result.Description);
            })
            .AddEndpointFilter<TechnicalValidationFilter<CreateActivateCommand>>()
            .Produces<ActivateResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        
        group.MapGet("GetById/{activateId}", async (long activateId, ISender sender) =>
            {
                var result = await _activatesService.GetActivateAsync(activateId, sender);
                return result.ToApiResult();
            })
            .Produces<ActivateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("Update", async ([FromBody] UpdateActivateCommand command, ISender sender) =>
            {
                var result = await _activatesService.UpdateActivateAsync(command, sender);
                return result.ToApiResult(result.Description);
            })
            .AddEndpointFilter<TechnicalValidationFilter<UpdateActivateCommand>>()
            .Produces<ActivateResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}