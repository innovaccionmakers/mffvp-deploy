using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.TrustOperations.CreateTrustOperation;

namespace Trusts.Presentation.TrustOperations;

internal sealed class CreateTrustOperation : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("trustoperations", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateTrustOperationCommand(
                    request.CustomerDealId,
                    request.TrustId,
                    request.Amount
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.TrustOperations);
    }

    internal sealed class Request
    {
        public Guid CustomerDealId { get; init; }
        public Guid TrustId { get; init; }
        public decimal Amount { get; init; }
    }
}