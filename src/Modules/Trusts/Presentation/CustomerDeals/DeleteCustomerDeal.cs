using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.CustomerDeals.DeleteCustomerDeal;

namespace Trusts.Presentation.CustomerDeals;

internal sealed class DeleteCustomerDeal : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("customerdeals/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteCustomerDealCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.CustomerDeals);
    }
}