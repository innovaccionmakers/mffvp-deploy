using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.Trusts.DeleteTrust;

namespace Trusts.Presentation.Trusts;

internal sealed class DeleteTrust : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("trusts/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteTrustCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.Trusts);
    }
}