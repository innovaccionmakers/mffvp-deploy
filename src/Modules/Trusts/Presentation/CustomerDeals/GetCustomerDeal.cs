using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.CustomerDeals.GetCustomerDeal;

namespace Trusts.Presentation.CustomerDeals;

internal sealed class GetCustomerDeal : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("customerdeals/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetCustomerDealQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.CustomerDeals);
    }
}