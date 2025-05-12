using Activations.Integrations.Affiliates.GetAffiliates;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Activations.Presentation.Affiliates;

internal sealed class GetAffiliates : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("affiliates", async (ISender sender) =>
            {
                var result = await sender.Send(new GetAffiliatesQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Affiliates);
    }
}