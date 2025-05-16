using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Products;

namespace MFFVP.Api.BffWeb.Products
{
    public sealed class ProductsEndpoints : IEndpoint
    {
        private readonly IPlansService _plansService;
        private readonly IAlternativesService _alternativesService;
        private readonly IObjectivesService _objectivesService;
        private readonly IPortfoliosService _portfoliosService;

        public ProductsEndpoints(
            IPlansService plansService,            
            IAlternativesService alternativesService,            
            IObjectivesService objectivesService,            
            IPortfoliosService portfoliosService            
        )
        {
            _plansService = plansService;
            _alternativesService = alternativesService;
            _objectivesService = objectivesService;
            _portfoliosService = portfoliosService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var portfoliosEndpoints = new Portfolios.PortfoliosEndpoints(_portfoliosService);
            portfoliosEndpoints.MapEndpoint(app);
        }
    }
}