using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Activations;

namespace MFFVP.Api.BffWeb.Activations
{
    public sealed class ActivationsEndpoints : IEndpoint
    {
        private readonly IMeetsPensionRequirementsService _meetspensionrequirementsService;
        private readonly IAffiliatesService _affiliatesService;

        public ActivationsEndpoints(         
            IMeetsPensionRequirementsService meetspensionrequirementsService,
            IAffiliatesService affiliatesService
        )
        {
            _meetspensionrequirementsService = meetspensionrequirementsService;
            _affiliatesService = affiliatesService;
        }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            var affiliatesEndpoints = new Affiliates.AffiliatesEndpoints(_affiliatesService);
            affiliatesEndpoints.MapEndpoint(app);
            var meetspensionrequirementsEndpoints = new MeetsPensionRequirements.MeetsPensionRequirementsEndpoints(_meetspensionrequirementsService);
            meetspensionrequirementsEndpoints.MapEndpoint(app);
        }
    }
}