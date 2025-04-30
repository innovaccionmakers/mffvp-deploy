using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Contributions;

namespace MFFVP.Api.BffWeb.Contributions
{
    public sealed class ContributionsEndpoints : IEndpoint
    {
        private readonly ITrustsService _trustsService;
        private readonly IClientOperationsService _clientoperationsService;
        private readonly ITrustOperationsService _trustoperationsService;

        public ContributionsEndpoints(
            ITrustsService trustsService,            
            IClientOperationsService clientoperationsService,            
            ITrustOperationsService trustoperationsService            
        )
        {
            _trustsService = trustsService;
            _clientoperationsService = clientoperationsService;
            _trustoperationsService = trustoperationsService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var trustsEndpoints = new Trusts.TrustsEndpoints(_trustsService);
            trustsEndpoints.MapEndpoint(app);
            var clientoperationsEndpoints = new ClientOperations.ClientOperationsEndpoints(_clientoperationsService);
            clientoperationsEndpoints.MapEndpoint(app);
            var trustoperationsEndpoints = new TrustOperations.TrustOperationsEndpoints(_trustoperationsService);
            trustoperationsEndpoints.MapEndpoint(app);
        }
    }
}