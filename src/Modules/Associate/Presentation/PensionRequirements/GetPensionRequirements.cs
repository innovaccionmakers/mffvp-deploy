using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Associate.Integrations.PensionRequirements.GetPensionRequirements;

namespace Associate.Presentation.PensionRequirements
{
    internal sealed class GetPensionRequirements : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("pensionrequirements", async (ISender sender) =>
            {
                var result = await sender.Send(new GetPensionRequirementsQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.PensionRequirements);
        }
    }
}