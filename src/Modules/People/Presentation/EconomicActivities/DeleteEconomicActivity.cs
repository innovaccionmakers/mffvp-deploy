using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.EconomicActivities.DeleteEconomicActivity;

namespace People.Presentation.EconomicActivities
{
    internal sealed class DeleteEconomicActivity : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("economicactivities/{id:string}", async (string id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteEconomicActivityCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.EconomicActivities);
        }
    }
}