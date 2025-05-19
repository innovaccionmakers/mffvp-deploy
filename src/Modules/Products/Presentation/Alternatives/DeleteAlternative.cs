using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Alternatives.DeleteAlternative;

namespace Products.Presentation.Alternatives;

internal sealed class DeleteAlternative : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("alternatives/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteAlternativeCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.Alternatives);
    }
}