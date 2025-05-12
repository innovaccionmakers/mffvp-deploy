using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Activations.Integrations.MeetsPensionRequirements.CreateMeetsPensionRequirement;
using Activations.Domain.Affiliates;

namespace Activations.Presentation.MeetsPensionRequirements
{
    internal sealed class CreateMeetsPensionRequirement : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("meetspensionrequirements", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateMeetsPensionRequirementCommand(
                    request.AffiliateId, 
                    request.StartDate, 
                    request.ExpirationDate, 
                    request.CreationDate, 
                    request.State
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.MeetsPensionRequirements);
        }

        internal sealed class Request
        {
            public int AffiliateId { get; init; }
            public DateTime StartDate { get; init; }
            public DateTime ExpirationDate { get; init; }
            public DateTime CreationDate { get; init; }
            public string State { get; init; }
        }
    }
}