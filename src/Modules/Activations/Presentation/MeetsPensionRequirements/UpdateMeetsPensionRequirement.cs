using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Activations.Integrations.MeetsPensionRequirements.UpdateMeetsPensionRequirement;

namespace Activations.Presentation.MeetsPensionRequirements
{
    internal sealed class UpdateMeetsPensionRequirement : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("meetspensionrequirements/{id:int}", async (int id, Request request, ISender sender) =>
            {
                var command = new UpdateMeetsPensionRequirementCommand(
                    id,
                    request.NewActivationId, 
                    request.NewStartDate, 
                    request.NewExpirationDate, 
                    request.NewCreationDate, 
                    request.NewState
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.MeetsPensionRequirements);
        }

        internal sealed class Request
        {
            public int NewActivationId { get; set; }
            public DateTime NewStartDate { get; set; }
            public DateTime NewExpirationDate { get; set; }
            public DateTime NewCreationDate { get; set; }
            public string NewState { get; set; }
        }
    }
}