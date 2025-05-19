using MediatR;
using People.Integrations.People.UpdatePerson;
using People.Integrations.People;
using Common.SharedKernel.Presentation.Results;
using Microsoft.AspNetCore.Mvc;
using MFFVP.Api.Application.People;
using People.Integrations.People.CreatePerson;

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

        group.MapGet("GetById/{personId}", async (long personId, ISender sender) =>
            {
                var result = await _peopleService.GetPersonAsync(personId, sender);
                return result.ToApiResult();
            })
            .Produces<PersonResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status500InternalServerError);
    }
}