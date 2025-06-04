using MediatR;
using People.Integrations.People.UpdatePerson;
using People.Integrations.People;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using MFFVP.Api.Application.People;

namespace MFFVP.Api.BffWeb.People.People;

public sealed class PeopleEndpoints
{
    private readonly IPeopleService _peopleService;

    public PeopleEndpoints(IPeopleService peopleService)
    {
        _peopleService = peopleService;
    }

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("bffWeb/people/people")
            .WithTags("BFF Web - People")
            .WithOpenApi();
    }
}