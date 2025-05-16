using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Portfolios.DeletePortfolio;

namespace Products.Presentation.Portfolios
{
    internal sealed class DeletePortfolio : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("portfolios/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new DeletePortfolioCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.Portfolios);
        }
    }
}