using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Closing;

namespace MFFVP.Api.BffWeb.Closing
{
    public sealed class ClosingEndpoints : IEndpoint
    {
        private readonly IProfitLossService _profitLossService;

        public ClosingEndpoints(IProfitLossService profitLossService)
        {
            _profitLossService = profitLossService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var profitLossEndpoints = new ProfitLosses.ProfitLossEndpoints(_profitLossService);
            profitLossEndpoints.MapEndpoint(app);
        }
    }
}