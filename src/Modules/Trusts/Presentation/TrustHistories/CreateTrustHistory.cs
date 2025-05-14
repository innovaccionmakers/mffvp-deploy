using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.TrustHistories.CreateTrustHistory;

namespace Trusts.Presentation.TrustHistories;

internal sealed class CreateTrustHistory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("trusthistories", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateTrustHistoryCommand(
                    request.TrustId,
                    request.Earnings,
                    request.Date,
                    request.SalesUserId
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.TrustHistories);
    }

    internal sealed class Request
    {
        public long TrustId { get; init; }
        public decimal Earnings { get; init; }
        public DateTime Date { get; init; }
        public string SalesUserId { get; init; }
    }
}