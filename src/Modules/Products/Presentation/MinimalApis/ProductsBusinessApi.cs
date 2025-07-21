using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;

using MediatR;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Products.Integrations.Objectives.CreateObjective;
using Products.Integrations.Objectives.GetObjectives;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.GetPortfolio;
using Products.Integrations.Portfolios.GetPortfolios;

namespace Products.Presentation.MinimalApis;

public static class ProductsBusinessApi
{
    public static void MapProductsBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("api/v1/FVP/Product")
                .WithTags("Product")
                .WithOpenApi()
                .RequireAuthorization();

        group.MapGet(
                "GetGoals",
                async (
                    [FromQuery] string? typeId,
                    [FromQuery] string? identification,
                    [FromQuery] string? status,
                    ISender sender
                ) =>
                {
                    var st = MapStatus(status);
                    var result = await sender.Send(new GetObjectivesQuery(typeId, identification, st));
                    return result.Match(
                        Results.Ok,
                        r => r.Error.Type == ErrorType.Validation
                            ? ApiResults.Failure(r)
                            : ApiResults.Problem(r)
                    );
                }
            )
            .WithName("GetGoals")
            .WithSummary("Retorna la lista de objetivos de un usuario")
            .WithDescription("""
                             **Ejemplo de llamada (query):**

                             ```http
                             GET /FVP/Product/GetGoals?typeId=C&identification=123456789&status=A
                             ```

                             - `typeId`: C (Ciudadanía)  
                             - `identification`: 27577533  
                             - `status`: A (Activo)  
                             """)
            .WithOpenApi(operation =>
            {
                var p0 = operation.Parameters.First(p => p.Name == "typeId");
                p0.Description = "Tipo de identificación del cliente (C=Ciudadanía, R=RUC, P=Pasaporte)";
                p0.Example = new OpenApiString("C");

                var p1 = operation.Parameters.First(p => p.Name == "identification");
                p1.Description = "Número de documento";
                p1.Example = new OpenApiString("27577533");

                var p2 = operation.Parameters.First(p => p.Name == "status");
                p2.Description = "Estado de los objetivos (A=Activo, I=Inactivo, T=Todos)";
                p2.Example = new OpenApiString("A");

                p2.Schema ??= new OpenApiSchema { Type = "string" };

                if (p2.Schema.Enum is null || p2.Schema.Enum.Count == 0)
                {
                    p2.Schema.Enum = new List<IOpenApiAny>
                    {
                        new OpenApiString("A"),
                        new OpenApiString("I"),
                        new OpenApiString("T")
                    };
                }

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
                    var resultado = await sender.Send(comando);
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

        group.MapGet(
                    "Portfolios/GetById",
                    async (
                        [FromQuery] int portfolioId,
                        ISender sender
                    ) =>
                    {
                        var result = await sender.Send(new GetPortfolioQuery(portfolioId));
                        return result.ToApiResult();
                    }
                )
                .WithName("GetPortfolioById")
                .WithSummary("Obtiene un portafolio por su identificador")
                .WithDescription("""
                                 **Ejemplo de llamada:**

                                 ```http
                                 GET /FVP/products/portfolios/GetById?portfolioId=123
                                 ```

                                 - `portfolioId`: Identificador del portafolio (e.g., 123)
                                 """)
                .WithOpenApi(operation =>
                {
                    var p = operation.Parameters.First(p => p.Name == "portfolioId");
                    p.Description = "Identificador único del portafolio";
                    p.Example = new OpenApiInteger(123);
                    return operation;
                })
                .Produces<PortfolioResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet(
                    "Portfolios/GetAllPortfolios",
                    async (ISender sender) =>
                    {
                        var result = await sender.Send(new GetPortfoliosQuery());
                        return result.Value;
                    }
                )
                .WithName("GetAllPortfolios")
                .WithSummary("Obtiene todos los portafolios")
                .WithDescription("""
                                 **Ejemplo de llamada:**

                                 ```http
                                 GET /FVP/products/portfolios/GetAllPortfolios
                                 ```
                                 """)
                .Produces<IReadOnlyCollection<PortfolioResponse>>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
    }



    private static StatusType MapStatus(string? raw) =>
        string.IsNullOrWhiteSpace(raw)
            ? StatusType.Missing
            : raw.Trim().ToUpperInvariant() switch
            {
                "A" => StatusType.A,
                "I" => StatusType.I,
                "T" => StatusType.T,
                _ => StatusType.Unknown
            };
}
