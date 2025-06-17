using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using MFFVP.Api.Application.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Products.Integrations.Objectives.CreateObjective;
using Products.Integrations.Objectives.GetObjectives;

namespace MFFVP.Api.BffWeb.Products.Objectives;

public sealed class ObjectivesEndpoints
{
    private readonly IObjectivesService _objectivesService;

    public ObjectivesEndpoints(IObjectivesService objectivesService)
    {
        _objectivesService = objectivesService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("bffWeb/FVP/Product")
            .WithTags("BFF Web - Objectives")
            .WithOpenApi();

        group.MapGet(
                "GetGoals",
                async (
                    [FromQuery] string typeId,
                    [FromQuery] string identification,
                    [FromQuery] StatusType status,
                    ISender sender
                ) =>
                {
                    var result = await _objectivesService
                        .GetObjectivesAsync(typeId, identification, status, sender);
                    return result.Match(Results.Ok, ApiResults.Problem);
                }
            )
            .WithName("GetGoals")
            .WithSummary("Retorna la lista de objetivos de un usuario")
            .WithDescription("""
                             **Ejemplo de llamada (query):**

                             ```http
                             GET /bffWeb/FVP/Product/GetGoals?typeId=C&identification=123456789&status=A
                             ```

                             - `typeId`: C (Ciudadanía)  
                             - `identification`: 123456789  
                             - `status`: A (Activo)  
                             """)
            .WithOpenApi(operation =>
            {
                var p0 = operation.Parameters.First(p => p.Name == "typeId");
                p0.Description = "Tipo de identificación del cliente (C=Ciudadanía, R=RUC, P=Pasaporte)";
                p0.Example     = new OpenApiString("C");

                var p1 = operation.Parameters.First(p => p.Name == "identification");
                p1.Description = "Número de documento";
                p1.Example     = new OpenApiString("123456789");

                var p2 = operation.Parameters.First(p => p.Name == "status");
                p2.Description = "Estado de los objetivos (A=Activo, I=Inactivo, T=Todos)";
                p2.Example     = new OpenApiString("A");

                return operation;
            })
            .Produces<IReadOnlyCollection<ObjectiveItem>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPost(
                "Goals",
                async (
                    CreateObjectiveCommand comando,
                    ISender sender
                ) =>
                {
                    var resultado = await _objectivesService
                        .CreateObjectiveAsync(comando, sender);
                    return resultado.ToApiResult();
                }
            )
            .WithName("Goals")
            .WithSummary("Crea un nuevo objetivo de ahorro para un cliente")
            .WithDescription("""
                **Ejemplo de petición (application/json):**
                ```json
                {
                  "TipoId": "CC",
                  "Identificacion": "123456789",
                  "IdAlternativa": "ALT001",
                  "TipoObjetivo": "Ahorro",
                  "NombreObjetivo": "Viaje a Cartagena",
                  "OficinaApertura": "001",
                  "OficinaActual": "001",
                  "Comercial": "COM123"
                }
                ```
                """)
            .AddEndpointFilter<TechnicalValidationFilter<CreateObjectiveCommand>>()
            .Accepts<CreateObjectiveCommand>("application/json")
            .Produces<ObjectiveResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}