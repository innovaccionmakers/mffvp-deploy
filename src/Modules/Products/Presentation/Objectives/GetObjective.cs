using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Objectives.GetObjective;

namespace Products.Presentation.Objectives;

internal sealed class GetObjective : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("objectives/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new GetObjectiveQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Objectives);
    }
}