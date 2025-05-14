using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.TrustHistories.GetTrustHistory;

namespace Trusts.Presentation.TrustHistories;

internal sealed class GetTrustHistory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("trusthistories/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new GetTrustHistoryQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.TrustHistories);
    }
}