using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Operations;

namespace MFFVP.Api.BffWeb.Operations;

public class OperationsEndpoints : IEndpoint
{
    private readonly IOperationsService _operationsService;

    public OperationsEndpoints(IOperationsService operationsService)
    {
        _operationsService = operationsService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var oeprationsEndpoints = new Operations.OperationsEndpoints(_operationsService);
        oeprationsEndpoints.MapEndpoint(app);
    }
}