using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Associate;
using MFFVP.Api.BffWeb.Associate.Activates;

namespace MFFVP.Api.BffWeb.Associate;

public sealed class AssociateEndpoints : IEndpoint
{
    private readonly IActivatesService _activatesService;
    private readonly IPensionRequirementsService _pensionRequirementsService;

    public AssociateEndpoints(IActivatesService activatesService, IPensionRequirementsService pensionRequirementsService)
        => (_activatesService, _pensionRequirementsService) = (activatesService, pensionRequirementsService);

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var activatesEndpoints = new ActivatesEndpoints(_activatesService, _pensionRequirementsService);
        activatesEndpoints.MapEndpoint(app);
    }
}