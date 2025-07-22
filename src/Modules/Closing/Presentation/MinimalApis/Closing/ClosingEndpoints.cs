
using Closing.Domain.Routes;
using Closing.Integrations.Closing.RunClosing;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Closing.Presentation.MinimalApis.Closing;

public static class ClosingEndpoints
{
    public static void MapClosingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(Routes.RunClosing)
            .WithTags(TagName.TagClosing)
            .WithOpenApi();

        group.MapPost(
               NameEndpoints.RunClosing,
                async (
                    [FromBody] RunClosingCommand request,
                    ISender sender
                ) =>
                {
                    var result = await sender.Send(request);
                    return result.ToApiResult();
                }
            )
            .WithName(NameEndpoints.RunClosing)
            .WithSummary(Summary.RunClosing)
            .WithDescription(Description.RunClosing)
            .WithOpenApi(operation =>
            {
                operation.RequestBody.Description = RequestBodyDescription.RunClosing;
                return operation;
            })
            .AddEndpointFilter<TechnicalValidationFilter<RunClosingCommand>>()
            .Produces<bool>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}
