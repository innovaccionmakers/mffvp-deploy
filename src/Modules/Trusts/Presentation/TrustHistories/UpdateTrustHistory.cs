using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.TrustHistories.UpdateTrustHistory;

namespace Trusts.Presentation.TrustHistories;

internal sealed class UpdateTrustHistory : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("trusthistories/{id:long}", async (long id, Request request, ISender sender) =>
            {
                var command = new UpdateTrustHistoryCommand(
                    id,
                    request.NewTrustId,
                    request.NewEarnings,
                    request.NewDate,
                    request.NewSalesUserId
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.TrustHistories);
    }

    internal sealed class Request
    {
        public long NewTrustId { get; set; }
        public decimal NewEarnings { get; set; }
        public DateTime NewDate { get; set; }
        public string NewSalesUserId { get; set; }
    }
}