using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.CustomerDeals.GetCustomerDeals;

namespace Trusts.Presentation.CustomerDeals;

internal sealed class GetCustomerDeals : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("customerdeals", async (ISender sender) =>
            {
                var result = await sender.Send(new GetCustomerDealsQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.CustomerDeals);
    }
}