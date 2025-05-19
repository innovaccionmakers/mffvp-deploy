using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Objectives.DeleteObjective;

namespace Products.Presentation.Objectives;

internal sealed class DeleteObjective : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("objectives/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteObjectiveCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.Objectives);
    }
}