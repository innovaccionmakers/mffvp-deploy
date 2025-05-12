using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.CustomerDeals.UpdateCustomerDeal;

namespace Trusts.Presentation.CustomerDeals;

internal sealed class UpdateCustomerDeal : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("customerdeals/{id:guid}", async (Guid id, Request request, ISender sender) =>
            {
                var command = new UpdateCustomerDealCommand(
                    id,
                    request.NewDate,
                    request.NewAffiliateId,
                    request.NewObjectiveId,
                    request.NewPortfolioId,
                    request.NewConfigurationParamId,
                    request.NewAmount
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.CustomerDeals);
    }

    internal sealed class Request
    {
        public DateTime NewDate { get; set; }
        public int NewAffiliateId { get; set; }
        public int NewObjectiveId { get; set; }
        public int NewPortfolioId { get; set; }
        public int NewConfigurationParamId { get; set; }
        public decimal NewAmount { get; set; }
    }
}