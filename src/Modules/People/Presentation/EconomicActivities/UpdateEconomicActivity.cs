using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.EconomicActivities.UpdateEconomicActivity;

namespace People.Presentation.EconomicActivities;

internal sealed class UpdateEconomicActivity : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("economicactivities/{id:string}", async (string id, Request request, ISender sender) =>
            {
                var command = new UpdateEconomicActivityCommand(
                    id,
                    request.NewEconomicActivityId,
                    request.NewDescription,
                    request.NewCiiuCode,
                    request.NewDivisionCode,
                    request.NewDivisionName,
                    request.NewGroupName,
                    request.NewClassCode,
                    request.NewStandardCode
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.EconomicActivities);
    }

    internal sealed class Request
    {
        public string NewEconomicActivityId { get; set; }
        public string NewDescription { get; set; }
        public string NewCiiuCode { get; set; }
        public string NewDivisionCode { get; set; }
        public string NewDivisionName { get; set; }
        public string NewGroupName { get; set; }
        public string NewClassCode { get; set; }
        public string NewStandardCode { get; set; }
    }
}