using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.Trusts.UpdateTrust;

namespace Trusts.Presentation.Trusts;

internal sealed class UpdateTrust : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("trusts/{id:long}", async (long id, Request request, ISender sender) =>
            {
                var command = new UpdateTrustCommand(
                    id,
                    request.NewAffiliateId,
                    request.NewClientId,
                    request.NewCreationDate,
                    request.NewObjectiveId,
                    request.NewPortfolioId,
                    request.NewTotalBalance,
                    request.NewTotalUnits,
                    request.NewPrincipal,
                    request.NewEarnings,
                    request.NewTaxCondition,
                    request.NewContingentWithholding,
                    request.NewEarningsWithholding,
                    request.NewAvailableAmount,
                    request.NewContingentWithholdingPercentage
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Trusts);
    }

    internal sealed class Request
    {
        public int NewAffiliateId { get; set; }
        public int NewClientId { get; set; }
        public DateTime NewCreationDate { get; set; }
        public int NewObjectiveId { get; set; }
        public int NewPortfolioId { get; set; }
        public decimal NewTotalBalance { get; set; }
        public int NewTotalUnits { get; set; }
        public decimal NewPrincipal { get; set; }
        public decimal NewEarnings { get; set; }
        public int NewTaxCondition { get; set; }
        public decimal NewContingentWithholding { get; set; }
        public decimal NewEarningsWithholding { get; set; }
        public decimal NewAvailableAmount { get; set; }
        public decimal NewContingentWithholdingPercentage { get; set; }
    }
}