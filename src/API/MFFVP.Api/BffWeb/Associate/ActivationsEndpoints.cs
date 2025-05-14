using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Associate;
using MFFVP.Api.BffWeb.Associate.Activates;

namespace MFFVP.Api.BffWeb.Associate;

public sealed class AssociateEndpoints : IEndpoint
{
    private readonly IActivatesService _activatesService;

    public AssociateEndpoints(IActivatesService activatesService)
        => _activatesService = activatesService;

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var activatesEndpoints = new ActivatesEndpoints(_activatesService);
        activatesEndpoints.MapEndpoint(app);
    }
}