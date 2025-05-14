using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.TrustHistories.DeleteTrustHistory;

namespace Trusts.Presentation.TrustHistories;

internal sealed class DeleteTrustHistory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("trusthistories/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteTrustHistoryCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.TrustHistories);
    }
}