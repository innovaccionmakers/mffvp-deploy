using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Associate.Integrations.PensionRequirements.GetPensionRequirements;
using Associate.Integrations.PensionRequirements;

namespace Associate.Presentation.PensionRequirements
{
    internal sealed class GetPensionRequirements : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("GetPensionRequirements", async (ISender sender) =>
            {
                var result = await sender.Send(new GetPensionRequirementsQuery());
                return result;
            })
            .Produces <IReadOnlyCollection<PensionRequirementResponse>>()
            .ProducesProblem(StatusCodes.Status500InternalServerError);
        }
    }
}