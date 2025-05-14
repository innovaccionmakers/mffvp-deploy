using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.Trusts.GetTrust;

namespace Trusts.Presentation.Trusts;

internal sealed class GetTrust : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("trusts/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new GetTrustQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Trusts);
    }
}