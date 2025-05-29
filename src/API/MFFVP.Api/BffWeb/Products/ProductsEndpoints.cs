using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Products;

namespace MFFVP.Api.BffWeb.Products
{
    public sealed class ProductsEndpoints : IEndpoint
    {
        private readonly IObjectivesService _objectivesService;
        private readonly IPortfoliosService _portfoliosService;

        public ProductsEndpoints(
            IObjectivesService objectivesService,            
            IPortfoliosService portfoliosService            
        )
        {
            _objectivesService = objectivesService;
            _portfoliosService = portfoliosService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var portfoliosEndpoints = new Portfolios.PortfoliosEndpoints(_portfoliosService);
            portfoliosEndpoints.MapEndpoint(app);
            var objectivesEndpoints = new Objectives.ObjectivesEndpoints(_objectivesService);
            objectivesEndpoints.MapEndpoint(app);
        }
    }
}