using Associate.Integrations.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.Activates.GetActivate;
using Associate.Integrations.Activates.GetActivates;
using Associate.Integrations.Activates.UpdateActivate;
using Associate.Integrations.PensionRequirements;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Associate.Integrations.PensionRequirements.GetPensionRequirements;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
using Associate.Integrations.Balances.AssociateBalancesById;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;

namespace Associate.Presentation.MinimalApis;

public static class AssociateBusinessApi
{
    public static void MapAssociateBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/v1/FVP/Associate")
            .WithTags("Associate")
            .WithOpenApi()
            .RequireAuthorization();

        group.MapGet("GetAssociates",
            [Authorize(Policy = "fvp:associate:activates:view")]
            async (ISender sender) =>
            {
                var result = await sender.Send(new GetActivatesQuery());
                return result.Value;
            })
            .WithSummary("Retorna una lista de activaciones")
            .Produces<IReadOnlyCollection<ActivateResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("Activate",
            async ([Microsoft.AspNetCore.Mvc.FromBody] CreateActivateCommand request, ISender sender) =>
            {
                var result = await sender.Send(request);
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
                    var result = await sender.Send(new GetActivateQuery(activateId));
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
            var result = await sender.Send(command);
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
            var result = await sender.Send(new GetPensionRequirementsQuery());
            return result;
        })
            .WithSummary("Retorna lista de requerimientos de pensioó")
            .Produces<IReadOnlyCollection<PensionRequirementResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost("PensionRequirements", async ([FromBody] CreatePensionRequirementCommand request, ISender sender) =>
        {
            var result = await sender.Send(request);
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
            var result = await sender.Send(command);
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

        group.MapGet(
                "BalancesById",
                async (
                    [FromQuery(Name = "tipoId")] string documentType,
                    [FromQuery(Name = "identificacion")] string identification,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(new AssociateBalancesByIdQuery(documentType, identification));
                    return result.Match(
                        Results.Ok,
                        r => r.Error.Type == ErrorType.Validation
                            ? ApiResults.Failure(r)
                            : ApiResults.Problem(r)
                    );
                }
            )
            .WithSummary("Obtiene los saldos consolidados de un afiliado")
            .Produces<IReadOnlyCollection<AssociateBalanceWrapper>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
