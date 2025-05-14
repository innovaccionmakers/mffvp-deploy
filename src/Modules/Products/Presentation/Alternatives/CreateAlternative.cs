using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Alternatives.CreateAlternative;

namespace Products.Presentation.Alternatives
{
    internal sealed class CreateAlternative : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("alternatives", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateAlternativeCommand(
                    request.AlternativeTypeId, 
                    request.Name, 
                    request.Status, 
                    request.Description
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Alternatives);
        }

        internal sealed class Request
        {
            public int AlternativeTypeId { get; init; }
            public string Name { get; init; }
            public string Status { get; init; }
            public string Description { get; init; }
        }
    }
}