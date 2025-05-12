using Activations.Integrations.MeetsPensionRequirements.GetMeetsPensionRequirements;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Activations.Presentation.MeetsPensionRequirements;

internal sealed class GetMeetsPensionRequirements : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("meetspensionrequirements", async (ISender sender) =>
            {
                var result = await sender.Send(new GetMeetsPensionRequirementsQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.MeetsPensionRequirements);
    }
}