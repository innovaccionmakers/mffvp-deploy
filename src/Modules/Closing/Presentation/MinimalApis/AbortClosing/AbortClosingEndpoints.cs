using Closing.Domain.Routes;
using Closing.Integrations.Closing.AbortClosing;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Closing.Presentation.MinimalApis.AbortClosing;

public static class AbortClosingEndpoints
{
    public static void MapAbortClosingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(Routes.RunClosing)
            .WithTags(TagName.TagClosing)
            .WithOpenApi();

        group.MapPost(
                NameEndpoints.AbortClosing,
                async ([FromBody] AbortClosingCommand request, ISender sender) =>
                {
                    var result = await sender.Send(request);
                    return result.ToApiResult();
                })
            .WithName(NameEndpoints.AbortClosing)
            .WithSummary(Summary.AbortClosing)
            .WithDescription(Description.AbortClosing)
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Description = RequestBodyDescription.AbortClosing;
                return operation;
            })
            .AddEndpointFilter<TechnicalValidationFilter<AbortClosingCommand>>()
            .Produces<bool>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}