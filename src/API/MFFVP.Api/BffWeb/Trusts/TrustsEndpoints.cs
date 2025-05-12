using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.Trusts;
using MFFVP.Api.BffWeb.Trusts.FullContributions;

namespace MFFVP.Api.BffWeb.Trusts;

public sealed class TrustsEndpoints : IEndpoint
{
    private readonly ICustomerDealsService _customerdealsService;
    private readonly IFullContributionService _fullcontributionsService;
    private readonly IInputInfosService _inputinfosService;
    private readonly ITrustOperationsService _trustoperationsService;
    private readonly ITrustsService _trustsService;

    public TrustsEndpoints(
        ITrustsService trustsService,
        ICustomerDealsService customerdealsService,
        ITrustOperationsService trustoperationsService,
        IInputInfosService inputinfosService,
        IFullContributionService fullcontributionsService
    )
    {
        _trustsService = trustsService;
        _customerdealsService = customerdealsService;
        _trustoperationsService = trustoperationsService;
        _inputinfosService = inputinfosService;
        _fullcontributionsService = fullcontributionsService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        /*
        var trustsEndpoints = new Trusts.TrustsEndpoints(_trustsService);
        trustsEndpoints.MapEndpoint(app);
        var clientoperationsEndpoints = new ClientOperations.ClientOperationsEndpoints(_clientoperationsService);
        clientoperationsEndpoints.MapEndpoint(app);
        var trustoperationsEndpoints = new TrustOperations.TrustOperationsEndpoints(_trustoperationsService);
        trustoperationsEndpoints.MapEndpoint(app);
        */
        var fullcontributionsEndpoints = new FullContributionsEndpoints(_fullcontributionsService);
        fullcontributionsEndpoints.MapEndpoint(app);
    }
}