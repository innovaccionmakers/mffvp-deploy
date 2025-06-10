using Associate.Integrations.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.Activates.UpdateActivate;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using MFFVP.Api.Application.Associate;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;

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
        var group = app.MapGroup("BffWeb/FVP/Associate")
            .WithTags("BFF Web - Associate")
            .WithOpenApi();

        group.MapGet("GetAssociates", async (ISender sender) =>
            {
                var result = await _activatesService.GetActivatesAsync(sender);
                return result.Value;
            })
            .Produces<IReadOnlyCollection<ActivateResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("Activate", async ([FromBody] CreateActivateCommand request, ISender sender) =>
            {
                var result = await _activatesService.CreateActivateAsync(request, sender);
                return result.ToApiResult(result.Description);
            })
            .AddEndpointFilter<TechnicalValidationFilter<CreateActivateCommand>>()
            .Produces<ActivateResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        
        group.MapGet(
                "GetByIdAssociate",
                async (
                    [FromQuery] long activateId,
                    ISender sender
                ) =>
                {
                    var result = await _activatesService.GetActivateAsync(activateId, sender);
                    return result.ToApiResult();
                }
            )
            .WithName("GetActivateById")
            .WithSummary("Obtiene una activación por su identificador")
            .WithDescription("""
                             **Ejemplo de llamada:**

                             ```http
                             GET /BffWeb/FVP/Associate/GetByIdAssociate?activateId=456
                             ```

                             - `activateId`: Identificador de la activación (e.g., 456)
                             """)
            .WithOpenApi(operation =>
            {
                var p = operation.Parameters.First(p => p.Name == "activateId");
                p.Description = "Identificador único de la activación";
                p.Example     = new OpenApiInteger(456);
                return operation;
            })
            .Produces<ActivateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("PutAssociate", async ([FromBody] UpdateActivateCommand command, ISender sender) =>
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