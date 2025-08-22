using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Closing.Integrations.PreClosing.RunSimulation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Closing.Domain.Routes;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Domain.Auth.Permissions;
using Microsoft.AspNetCore.Authorization;

namespace Closing.Presentation.MinimalApis.PreClosing
{
    public static class PreClosingEndpoints
    {
        public static void MapPreclosingEndpoints(this WebApplication app)
        {
            var group = app.MapGroup(Routes.RunPreclosing)
                .WithTags(TagName.TagClosing)
                .WithOpenApi();

            group.MapPost(
                   NameEndpoints.RunPreclosing,
                   [Authorize(Policy = MakersPermissionsClosing.PolicyExecuteSimulation)]
                     async (
                        [FromBody] RunSimulationCommand request,
                        ISender sender,
                        HttpContext http,              // opcional, para saber si la respuesta ya empezó
                        CancellationToken cancellationToken           // 
                    ) =>
                    {
                        try
                        {
                            var result = await sender.Send(request, cancellationToken); // <- propaga cancelación
                            return result.ToApiResult();
                        }
                        catch (OperationCanceledException)
                        {
                            // Si el cliente aborta request, cae aquí.
                            if (!http.Response.HasStarted)
                                return Results.StatusCode(StatusCodes.Status499ClientClosedRequest); 
                            throw; // si ya empezó la respuesta, dejamos que el framework cierre la conexión
                        }
                    }
                )
                .WithName(NameEndpoints.RunPreclosing)
                .WithSummary(Summary.RunPreclosing)
                .WithDescription(Description.RunPreclosing)
                .WithOpenApi(operation =>
                {
                    operation.RequestBody.Description = RequestBodyDescription.RunPreclosing;
                    return operation;
                })
                .AddEndpointFilter<TechnicalValidationFilter<RunSimulationCommand>>()
                .Produces<SimulatedYieldResult>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .ProducesProblem(StatusCodes.Status500InternalServerError)
                .Produces(StatusCodes.Status499ClientClosedRequest); 
        }
    }
}