using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.People.GetPerson;

namespace People.Presentation.People;

internal sealed class GetPerson : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("persons/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new GetPersonQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Persons);
    }
}