using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.EconomicActivities.GetEconomicActivities;

namespace People.Presentation.EconomicActivities
{
    internal sealed class GetEconomicActivities : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("economicactivities", async (ISender sender) =>
            {
                var result = await sender.Send(new GetEconomicActivitiesQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.EconomicActivities);
        }
    }
}