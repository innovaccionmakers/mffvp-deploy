using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Trusts.Integrations.Trusts.CreateTrust;

namespace Trusts.Presentation.Trusts;

internal sealed class CreateTrust : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("trusts", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateTrustCommand(
                    request.AffiliateId,
                    request.ClientId,
                    request.ObjectiveId,
                    request.PortfolioId,
                    request.TotalBalance,
                    request.TotalUnits,
                    request.Principal,
                    request.Earnings,
                    request.TaxCondition,
                    request.ContingentWithholding
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Trusts);
    }

    internal sealed class Request
    {
        public int AffiliateId { get; init; }
        public int ClientId { get; init; }
        public int ObjectiveId { get; init; }
        public int PortfolioId { get; init; }
        public decimal TotalBalance { get; init; }
        public int TotalUnits { get; init; }
        public decimal Principal { get; init; }
        public decimal Earnings { get; init; }
        public int TaxCondition { get; init; }
        public int ContingentWithholding { get; init; }
    }
}