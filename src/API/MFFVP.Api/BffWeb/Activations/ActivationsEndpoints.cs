using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Activations;
using MFFVP.Api.BffWeb.Activations.Affiliates;
using MFFVP.Api.BffWeb.Activations.MeetsPensionRequirements;

namespace MFFVP.Api.BffWeb.Activations;

public sealed class ActivationsEndpoints : IEndpoint
{
    private readonly IAffiliatesService _affiliatesService;
    private readonly IMeetsPensionRequirementsService _meetspensionrequirementsService;

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
        var affiliatesEndpoints = new AffiliatesEndpoints(_affiliatesService);
        affiliatesEndpoints.MapEndpoint(app);
        var meetspensionrequirementsEndpoints = new MeetsPensionRequirementsEndpoints(_meetspensionrequirementsService);
        meetspensionrequirementsEndpoints.MapEndpoint(app);
    }
}