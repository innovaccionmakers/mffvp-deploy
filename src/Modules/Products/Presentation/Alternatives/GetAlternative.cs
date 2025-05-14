using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Products.Integrations.Alternatives.GetAlternative;

namespace Products.Presentation.Alternatives
{
    internal sealed class GetAlternative : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("alternatives/{id:long}", async (long id, ISender sender) =>
            {
                var result = await sender.Send(new GetAlternativeQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Alternatives);
        }
    }
}