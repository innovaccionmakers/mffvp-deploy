using Activations.Integrations.Affiliates.GetAffiliate;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Activations.Presentation.Affiliates;

internal sealed class GetAffiliate : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("affiliates/{id:int}", async (int id, ISender sender) =>
            {
                var result = await sender.Send(new GetAffiliateQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.Affiliates);
    }
}