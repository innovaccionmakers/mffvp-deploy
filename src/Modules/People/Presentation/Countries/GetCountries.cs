using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.Countries.GetCountries;

namespace People.Presentation.Countries
{
    internal sealed class GetCountries : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("countries", async (ISender sender) =>
            {
                var result = await sender.Send(new GetCountriesQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Countries);
        }
    }
}