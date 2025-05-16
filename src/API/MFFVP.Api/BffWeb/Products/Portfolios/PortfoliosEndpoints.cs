using Common.SharedKernel.Presentation.Filters;
using MediatR;
using Products.Integrations.Portfolios;
using Common.SharedKernel.Presentation.Results;
using MFFVP.Api.Application.Products;

namespace MFFVP.Api.BffWeb.Products.Portfolios
{
    public sealed class PortfoliosEndpoints
    {
        private readonly IPortfoliosService _portfoliosService;

        public PortfoliosEndpoints(IPortfoliosService portfoliosService)
        {
            _portfoliosService = portfoliosService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("bffWeb/products/portfolios")
                .WithTags("BFF Web - Portfolios")
                .WithOpenApi();

            group.MapGet("GetById/{portfolioId}", async (long portfolioId, ISender sender) =>
                {
                    var result = await _portfoliosService.GetPortfolioAsync(portfolioId, sender);
                    return result.ToApiResult();
                })
                .Produces<PortfolioResponse>(StatusCodes.Status200OK)
                .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}