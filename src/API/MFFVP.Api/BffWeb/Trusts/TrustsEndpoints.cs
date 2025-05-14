using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Trusts;

namespace MFFVP.Api.BffWeb.Trusts
{
    public sealed class TrustsEndpoints : IEndpoint
    {
        private readonly ITrustsService _trustsService;
        private readonly ITrustHistoriesService _trusthistoriesService;

        public TrustsEndpoints(
            ITrustsService trustsService,            
            ITrustHistoriesService trusthistoriesService            
        )
        {
            _trustsService = trustsService;
            _trusthistoriesService = trusthistoriesService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var trustsEndpoints = new Trusts.TrustsEndpoints(_trustsService);
            trustsEndpoints.MapEndpoint(app);
            var trusthistoriesEndpoints = new TrustHistories.TrustHistoriesEndpoints(_trusthistoriesService);
            trusthistoriesEndpoints.MapEndpoint(app);
        }
    }
}