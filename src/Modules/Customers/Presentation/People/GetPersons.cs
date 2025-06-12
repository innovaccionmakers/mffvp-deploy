using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Customers.Integrations.People.GetPersons;

namespace Customers.Presentation.People
{
    internal sealed class GetPersons : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("persons", async (ISender sender) =>
            {
                var result = await sender.Send(new GetPersonsQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Persons);
        }
    }
}