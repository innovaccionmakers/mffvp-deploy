using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain.Auth.Permissions;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

using Products.Domain.Routes;
using Products.Integrations.Objectives.CreateObjective;
using Products.Integrations.Objectives.GetObjectives;
using Products.Integrations.Objectives.UpdateObjective;
using Products.Integrations.Portfolios;
using Products.Integrations.Portfolios.GetPortfolio;
using Products.Integrations.Portfolios.GetPortfolioById;
using Products.Integrations.Portfolios.GetPortfolios;
using Products.Integrations.Portfolios.Queries;

namespace Products.Presentation.MinimalApis;

public static class ProductsBusinessApi
{
    public static void MapProductsBusinessEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(Routes.Product)
                .WithTags(TagName.TagProduct)
                .WithOpenApi()
                .RequireAuthorization();

        group.MapGet(
                "GetGoals",
                [Authorize(Policy = MakersPermissionsAffiliates.PolicyViewGoal)]
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
            .WithName(NameEndpoints.GetGoals)
            .WithSummary(Summary.GetGoals)
            .WithDescription(Description.GetGoals)
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
                [Authorize(Policy = MakersPermissionsAffiliates.PolicyCreateGoal)]
                async (
                    CreateObjectiveCommand comando,
                    ISender sender
                ) =>
                {
                    var resultado = await sender.Send(comando);
                    return resultado.ToApiResult();
                }
            )
            .WithName(NameEndpoints.Goals)
            .WithSummary(Summary.Goals)
            .WithDescription(Description.Goals)
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Description = RequestBodyDescription.Goals;
                return operation;
            })
            .AddEndpointFilter<TechnicalValidationFilter<CreateObjectiveCommand>>()
            .Accepts<CreateObjectiveCommand>("application/json")
            .Produces<ObjectiveResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapPut(
                "Goals",
                [Authorize(Policy = MakersPermissionsAffiliates.PolicyUpdateGoal)]
                async (
                    UpdateObjectiveCommand comando,
                    ISender sender
                ) =>
                {
                    var resultado = await sender.Send(comando);
                    return resultado.ToApiResult();
                }
            )
            .WithName("UpdateGoal")
            .WithSummary("Actualiza la información de un objetivo de ahorro para un cliente")
            .WithDescription("""
                **Ejemplo de petición (application/json):**
                ```json
                {
                  "IdObjetivo": 1,
                  "TipoObjetivo": "I",
                  "NombreObjetivo": "Viaje a Cartagena",
                  "OficinaApertura": "1",
                  "OficinaActual": "1",
                  "Comercial": "1",
                  "Estado": "I"
                }
                ```
                """)
            .AddEndpointFilter<TechnicalValidationFilter<UpdateObjectiveCommand>>()
            .Accepts<CreateObjectiveCommand>("application/json")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet(
                "Portfolios/GetById",
                [Authorize(Policy = MakersPermissionsAffiliates.PolicyViewGoal)]
                async (
                    [FromQuery] int portfolioId,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(new GetPortfolioQuery(portfolioId));
                    return result.ToApiResult();
                }
            )
            .WithName(NameEndpoints.GetPortfolioById)
            .WithSummary(Summary.GetPortfolioById)
            .WithDescription(Description.GetPortfolioById)
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
            .WithName(NameEndpoints.GetAllPortfolios)
            .WithSummary(Summary.GetAllPortfolios)
            .WithDescription(Description.GetAllPortfolios)
            .Produces<IReadOnlyCollection<PortfolioResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);

        group.MapGet(
                "Portfolios/GetPortfolioById",
                async (
                    [FromQuery] string codigoPortafolio,
                    ISender sender,
                    CancellationToken cancellationToken
                ) =>
                {
                    var query = new GetPortfolioByIdQuery(codigoPortafolio);
                    var result = await sender.Send(query, cancellationToken);
                    return result.ToApiResult();
                }
            )
            .WithName("GetPortfolioByHomologatedCode")
            .WithSummary("Obtiene un portafolio por su código homologado")
            .WithDescription("""
                **Ejemplo de llamada:**

                ```http
                GET /FVP/products/portfolios/GetPortfolioById?codigoPortafolio=PORT001
                ```

                - `codigoPortafolio`: Código homologado del portafolio (obligatorio)
                """)
            .WithOpenApi(operation =>
            {
                operation.Parameters[0].Description = "Código homologado del portafolio";
                return operation;
            })
            .Produces<GetPortfolioByIdResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
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
