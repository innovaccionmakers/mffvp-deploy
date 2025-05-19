using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.EconomicActivities.GetEconomicActivity;

namespace People.Presentation.EconomicActivities;

internal sealed class GetEconomicActivity : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("economicactivities/{id:string}", async (string id, ISender sender) =>
            {
                var result = await sender.Send(new GetEconomicActivityQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.EconomicActivities);
    }
}