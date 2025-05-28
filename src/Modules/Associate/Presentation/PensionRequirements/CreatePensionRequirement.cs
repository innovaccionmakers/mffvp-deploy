using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;

namespace Associate.Presentation.PensionRequirements
{
    internal sealed class CreatePensionRequirement : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("pensionrequirements", async (Request request, ISender sender) =>
            {
                // var result = await sender.Send(new CreatePensionRequirementCommand(
                //     request.AffiliateId, 
                //     request.StartDate, 
                //     request.ExpirationDate, 
                //     request.CreationDate, 
                //     request.Status
                // ));

                // return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.PensionRequirements);
        }

        internal sealed class Request
        {
            public DateTime AffiliateId { get; init; }
            public DateTime StartDate { get; init; }
            public DateTime ExpirationDate { get; init; }
            public DateTime CreationDate { get; init; }
            public string Status { get; init; }
        }
    }
}