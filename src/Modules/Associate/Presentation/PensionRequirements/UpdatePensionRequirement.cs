using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;

namespace Associate.Presentation.PensionRequirements
{
    internal sealed class UpdatePensionRequirement : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("pensionrequirements/{id:int}", async (int id, Request request, ISender sender) =>
            {
                var command = new UpdatePensionRequirementCommand(
                    request.IdentificationType, 
                    request.Identification, 
                    request.PensionRequirementId,
                    request.Status
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.PensionRequirements);
        }

        internal sealed class Request
        {
            public string IdentificationType { get; set; }
            public string Identification { get; set; }
            public int PensionRequirementId { get; set; }
            public string Status { get; set; }
        }
    }
}