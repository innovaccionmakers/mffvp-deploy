using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.CustomerDeals.CreateCustomerDeal;

namespace Trusts.Presentation.CustomerDeals;

internal sealed class CreateCustomerDeal : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("customerdeals", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateCustomerDealCommand(
                    request.Date,
                    request.AffiliateId,
                    request.ObjectiveId,
                    request.PortfolioId,
                    request.ConfigurationParamId,
                    request.Amount
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.CustomerDeals);
    }

    internal sealed class Request
    {
        public DateTime Date { get; init; }
        public int AffiliateId { get; init; }
        public int ObjectiveId { get; init; }
        public int PortfolioId { get; init; }
        public int ConfigurationParamId { get; init; }
        public decimal Amount { get; init; }
    }
}