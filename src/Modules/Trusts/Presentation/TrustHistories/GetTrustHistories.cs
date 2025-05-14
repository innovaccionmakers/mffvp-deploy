using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.TrustHistories.GetTrustHistories;

namespace Trusts.Presentation.TrustHistories;

internal sealed class GetTrustHistories : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("trusthistories", async (ISender sender) =>
            {
                var result = await sender.Send(new GetTrustHistoriesQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.TrustHistories);
    }
}