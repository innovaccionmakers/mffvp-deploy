using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using People.Integrations.EconomicActivities.CreateEconomicActivity;

namespace People.Presentation.EconomicActivities;

internal sealed class CreateEconomicActivity : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("economicactivities", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateEconomicActivityCommand(
                    request.EconomicActivityId,
                    request.Description,
                    request.CiiuCode,
                    request.DivisionCode,
                    request.DivisionName,
                    request.GroupName,
                    request.ClassCode,
                    request.StandardCode
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.EconomicActivities);
    }

    internal sealed class Request
    {
        public string EconomicActivityId { get; init; }
        public string Description { get; init; }
        public string CiiuCode { get; init; }
        public string DivisionCode { get; init; }
        public string DivisionName { get; init; }
        public string GroupName { get; init; }
        public string ClassCode { get; init; }
        public string StandardCode { get; init; }
    }
}