using Closing.Domain.Routes;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Domain.Auth.Permissions;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Text.Json;

namespace Closing.Presentation.MinimalApis.Closing;

public static class ClosingEndpointMappings
{
    //TODO: Considerar usar otra forma de mapear los endpoints
    public static void MapRunClosingEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost(
            NameEndpoints.PrepareClosing,
            [Authorize(Policy = MakersPermissionsClosing.PolicyExecuteClosure)]
            async (
                [FromBody] PrepareClosingCommand request, 
                ISender sender,
                 HttpContext http, 
                 CancellationToken cancellationToken
                ) =>
            {
                try
                {
                    var result = await sender.Send(request, cancellationToken);
                    return result.ToApiResult();
                }
                catch (OperationCanceledException)
                {
                    if (!http.Response.HasStarted)
                        return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
                    throw; 
                }
            })
            .WithName(NameEndpoints.PrepareClosing)
            .WithSummary(Summary.PrepareClosing)
            .WithDescription(Description.PrepareClosing)
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Description = RequestBodyDescription.PrepareClosing;
                return operation;
            })
            .AddEndpointFilter<TechnicalValidationFilter<PrepareClosingCommand>>()
            .Produces<PrepareClosingResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status499ClientClosedRequest);
    }

    public static void MapConfirmClosingEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost(
            NameEndpoints.ConfirmClosing,
            async (
                [FromBody] ConfirmClosingCommand request, 
                ISender sender,
                HttpContext http,
                CancellationToken cancellationToken

                ) =>
            {
                try
                {
                    var result = await sender.Send(request, cancellationToken); 
                    return result.ToApiResult();
                }
                catch (OperationCanceledException)
                {
                    if (!http.Response.HasStarted)
                        return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
                    throw;
                }
            })
            .WithName(NameEndpoints.ConfirmClosing)
            .WithSummary(Summary.ConfirmClosing)
            .WithDescription(Description.ConfirmClosing)
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Description = RequestBodyDescription.ConfirmClosing;
                return operation;
            })
            .AddEndpointFilter<TechnicalValidationFilter<ConfirmClosingCommand>>()
            .Produces<ConfirmClosingResult>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status499ClientClosedRequest);
    }
    /// <summary>
    /// Mapea el endpoint para cancelar el cierre de un portafolio. 
    /// Recibe dos tipos de contenido: application/json y text/plain.
    /// </summary>
    /// <param name="group"></param>
    public static void MapCancelClosingEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost(
            NameEndpoints.CancelClosing,
            async (
                ISender sender,
                HttpContext http,
                CancellationToken cancellationToken
                ) =>
            {
                try
                {
                    // 1) Leer el body como texto 
                    string body;
                    using (var reader = new StreamReader(http.Request.Body))
                    {
                        body = await reader.ReadToEndAsync(cancellationToken);
                    }

                    if (string.IsNullOrWhiteSpace(body))
                    {
                        return Results.BadRequest(new
                        {
                            title = "Body requerido",
                            detail = "El body no puede venir vacío."
                        });
                    }

                    // 2) Deserializar el JSON (que viene en el texto)
                    CancelClosingCommand? request;
                    try
                    {
                        request = JsonSerializer.Deserialize<CancelClosingCommand>(
                            body,
                            new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                    }
                    catch (JsonException)
                    {
                        return Results.BadRequest(new
                        {
                            title = "JSON inválido",
                            detail = "El contenido enviado no es un JSON válido."
                        });
                    }

                    if (request is null)
                    {
                        return Results.BadRequest(new
                        {
                            title = "Payload inválido",
                            detail = "No se pudo mapear el body a CancelClosingCommand."
                        });
                    }
                    var result = await sender.Send(request, cancellationToken);
                    return result.ToApiResult(); 
                }
                catch (OperationCanceledException)
                {
                    if (!http.Response.HasStarted)
                        return Results.StatusCode(StatusCodes.Status499ClientClosedRequest);
                    throw;
                }
            })
            .WithName(NameEndpoints.CancelClosing)
            .WithSummary(Summary.CancelClosing)
            .WithDescription(Description.CancelClosing)
            .WithOpenApi(operation =>
            {
                operation.RequestBody ??= new OpenApiRequestBody
                {
                    Required = true,
                    Description = RequestBodyDescription.CancelClosing
                };

                var cancelClosingSchema = new OpenApiSchema
                {
                    Type = "object",
                    Properties = new Dictionary<string, OpenApiSchema>
                    {
                        ["IdPortafolio"] = new OpenApiSchema
                        {
                            Type = "integer",
                            Format = "int32",
                            Description = "Identificador del portafolio"
                        },
                        ["FechaCierre"] = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "date-time",
                            Description = "Fecha de Cierre del portafolio"
                        }
                    },
                    Required = { "IdPortafolio", "FechaCierre" }
                };

                var example = new OpenApiObject
                {
                    ["IdPortafolio"] = new OpenApiInteger(1),
                    ["FechaCierre"] = new OpenApiString("2025-07-01T00:00:00Z")
                };

                operation.RequestBody.Content ??= new Dictionary<string, OpenApiMediaType>();
                operation.RequestBody.Content.Clear();

                operation.RequestBody.Content["application/json"] = new OpenApiMediaType
                {
                    Schema = cancelClosingSchema,
                    Example = example
                };

                operation.RequestBody.Content["text/plain"] = new OpenApiMediaType
                {
                    Schema = cancelClosingSchema,
                    Example = example
                };

                return operation;
            })
            .Produces<bool>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .Produces(StatusCodes.Status499ClientClosedRequest);
    }
}