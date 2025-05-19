using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Portfolios.GetPortfolios;

namespace Products.Presentation.Portfolios;

internal sealed class GetPortfolios : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("portfolios", async (ISender sender) =>
            {
                var result = await sender.Send(new GetPortfoliosQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Portfolios);
    }
}