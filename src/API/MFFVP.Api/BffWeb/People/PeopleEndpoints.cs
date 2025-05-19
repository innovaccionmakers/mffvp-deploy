using Common.SharedKernel.Presentation.Endpoints;
using MFFVP.Api.Application.People;

namespace MFFVP.Api.BffWeb.People;

public sealed class PeopleEndpoints : IEndpoint
{
    private readonly IPeopleService _peopleService;

    public PeopleEndpoints(
        IPeopleService peopleService
    )
    {
        _peopleService = peopleService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var peopleEndpoints = new People.PeopleEndpoints(_peopleService);
        peopleEndpoints.MapEndpoint(app);
    }
}