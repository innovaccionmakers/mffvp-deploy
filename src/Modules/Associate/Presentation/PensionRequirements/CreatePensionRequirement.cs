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
                var result = await sender.Send(new CreatePensionRequirementCommand(
                    request.IdentificationType, 
                    request.Identification, 
                    request.StartDateReqPen, 
                    request.EndDateReqPe
                ));

                return result;
            })
            .WithTags(Tags.PensionRequirements);
        }

        internal sealed class Request
        {
            public string IdentificationType { get; init; }
            public string Identification { get; init; }
            public DateTime StartDateReqPen { get; init; }
            public DateTime EndDateReqPe { get; init; }
        }
    }
}