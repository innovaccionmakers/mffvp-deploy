using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.Countries.UpdateCountry;

namespace People.Presentation.Countries
{
    internal sealed class UpdateCountry : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("countries/{id:int}", async (int id, Request request, ISender sender) =>
            {
                var command = new UpdateCountryCommand(
                    id,
                    request.NewName, 
                    request.NewShortName, 
                    request.NewDaneCode, 
                    request.NewStandardCode
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Countries);
        }

        internal sealed class Request
        {
            public string NewName { get; set; }
            public string NewShortName { get; set; }
            public string NewDaneCode { get; set; }
            public string NewStandardCode { get; set; }
        }
    }
}