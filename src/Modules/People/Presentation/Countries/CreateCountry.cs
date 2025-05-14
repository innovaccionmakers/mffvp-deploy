using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.Countries.CreateCountry;

namespace People.Presentation.Countries
{
    internal sealed class CreateCountry : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("countries", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateCountryCommand(
                    request.Name, 
                    request.ShortName, 
                    request.DaneCode, 
                    request.StandardCode
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Countries);
        }

        internal sealed class Request
        {
            public string Name { get; init; }
            public string ShortName { get; init; }
            public string DaneCode { get; init; }
            public string StandardCode { get; init; }
        }
    }
}