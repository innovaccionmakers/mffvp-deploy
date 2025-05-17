using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Portfolios.GetPortfolio;

namespace Products.Presentation.Portfolios;

internal sealed class GetPortfolio : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("portfolios/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new GetPortfolioQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Portfolios);
    }
}