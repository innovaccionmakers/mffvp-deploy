using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Contributions.Integrations.TrustOperations.GetTrustOperations;

namespace Contributions.Presentation.TrustOperations
{
    internal sealed class GetTrustOperations : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("trustoperations", async (ISender sender) =>
            {
                var result = await sender.Send(new GetTrustOperationsQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.TrustOperations);
        }
    }
}