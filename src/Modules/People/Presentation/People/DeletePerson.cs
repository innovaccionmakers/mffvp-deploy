using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.People.DeletePerson;

namespace People.Presentation.People
{
    internal sealed class DeletePerson : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("persons/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new DeletePersonCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.Persons);
        }
    }
}