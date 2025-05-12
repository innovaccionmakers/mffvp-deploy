using Activations.Integrations.MeetsPensionRequirements.GetMeetsPensionRequirement;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Activations.Presentation.MeetsPensionRequirements;

internal sealed class GetMeetsPensionRequirement : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("meetspensionrequirements/{id:int}", async (int id, ISender sender) =>
            {
                var result = await sender.Send(new GetMeetsPensionRequirementQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.MeetsPensionRequirements);
    }
}