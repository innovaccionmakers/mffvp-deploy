using Activations.Integrations.MeetsPensionRequirements.DeleteMeetsPensionRequirement;
using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Activations.Presentation.MeetsPensionRequirements;

internal sealed class DeleteMeetsPensionRequirement : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("meetspensionrequirements/{id:int}", async (int id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteMeetsPensionRequirementCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.MeetsPensionRequirements);
    }
}