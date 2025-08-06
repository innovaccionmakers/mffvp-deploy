using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Closing.Integrations.PreClosing.RunSimulation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Closing.Domain.Routes;
using Common.SharedKernel.Presentation.Filters;

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
                    async (
                        [FromBody] RunSimulationCommand request,
                        ISender sender
                    ) =>
                    {
                        var result = await sender.Send(request);
                        return result.ToApiResult();
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
                .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}