using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Alternatives.GetAlternatives;

namespace Products.Presentation.Alternatives
{
    internal sealed class GetAlternatives : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("alternatives", async (ISender sender) =>
            {
                var result = await sender.Send(new GetAlternativesQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Alternatives);
        }
    }
}