using Associate.Integrations.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.Activates.UpdateActivate;
using Associate.Integrations.PensionRequirements;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
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
    private readonly IPensionRequirementsService _pensionrequirementsService;


    public ActivatesEndpoints(IActivatesService activatesService, IPensionRequirementsService pensionrequirementsService)
    {
        _activatesService = activatesService;
        _pensionrequirementsService = pensionrequirementsService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("FVP/Associate")
            .WithTags("Associate")
            .WithOpenApi();

        group.MapGet("GetAssociates", async (ISender sender) =>
            {
                var result = await _activatesService.GetActivatesAsync(sender);
                return result.Value;
            })
            .WithSummary("Retorna una lista de activaciones")
            .Produces<IReadOnlyCollection<ActivateResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("Activate", async ([FromBody] CreateActivateCommand request, ISender sender) =>
            {
                var result = await _activatesService.CreateActivateAsync(request, sender);
                return result.ToApiResult(result.Description);
            })
            .WithSummary("Crea una activación")
            .WithDescription("""
                             **Ejemplo de petición (application/json):**
                             
                             *Ejemplo A*
                             ```json
                             {
                               "TipoId": "C",
                               "Identificacion": "01234567890",
                               "Pensionado": true
                             }
                             ```
                             *Ejemplo B*
                             ```json                             
                             {
                               "TipoId": "C",
                               "Identificacion": "01234567890",
                               "Pensionado": false,
                               "CumpleRequisitosPension": true,
                               "FechaInicioReqPen": "2025-06-18T14:44:54.188Z",
                               "FechaFinReqPen": "2026-06-18T14:44:54.188Z"
                             }
                             ```
                             """)
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
                p.Example = new OpenApiInteger(456);
                return operation;
            })
            .Produces<ActivateResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("PutAssociate", async ([FromBody] UpdateActivateCommand command, ISender sender) =>
            {
                var result = await _activatesService.UpdateActivateAsync(command, sender);
                return result.ToApiResult(result.Description);
            })
            .WithSummary("Actualiza una activación")
            .WithDescription("""
                             **Ejemplo de petición (application/json):**
                             ```json
                             {
                               "TipoId": "C",
                               "Identificacion": "01234567890",
                               "Pensionado": false
                             }
                             ```
                             """)
            .AddEndpointFilter<TechnicalValidationFilter<UpdateActivateCommand>>()
            .Produces<ActivateResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet("GetPensionRequirements", async (ISender sender) =>
            {
                var result = await _pensionrequirementsService.GetPensionRequirementsAsync(sender);
                return result;
            })
            .WithSummary("Retorna lista de requerimientos de pensioó")
            .Produces<IReadOnlyCollection<PensionRequirementResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("PensionRequirements", async ([FromBody] CreatePensionRequirementCommand request, ISender sender) =>
        {
            var result = await _pensionrequirementsService.CreatePensionRequirementAsync(request, sender);
            return result.ToApiResult(result.Description);
        })
        .WithSummary("Crea un requerimiento de pensión")
        .WithDescription("""
                             **Ejemplo de petición (application/json):**
                             ```json
                             {
                               "TipoId": "C",
                               "Identificacion": "01234567890",
                               "FechaInicioReqPen": "2025-06-18",
                               "FechaFinReqPen": "2025-06-19"
                             }
                             ```
                             """)
        .AddEndpointFilter<TechnicalValidationFilter<CreatePensionRequirementCommand>>()
        .Produces<PensionRequirementResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut("PutPensionRequirements", async ([FromBody] UpdatePensionRequirementCommand command, ISender sender) =>
        {
            var result = await _pensionrequirementsService.UpdatePensionRequirementAsync(command, sender);
            return result.ToApiResult(result.Description);
        })
        .WithSummary("Actualiza un requerimiento de pensión")
        .WithDescription("""
                             **Ejemplo de petición (application/json):**
                             ```json
                             {
                               "TipoId": "C",
                               "Identificacion": "01234567890",
                               "IdRequisitoPension": 13,
                               "Estado": "Inactive"
                             }
                             ```
                             """)
        .AddEndpointFilter<TechnicalValidationFilter<UpdatePensionRequirementCommand>>()
        .Produces<PensionRequirementResponse>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}