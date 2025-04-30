using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Contributions.Integrations.ClientOperations.GetClientOperations;

namespace Contributions.Presentation.ClientOperations
{
    internal sealed class GetClientOperations : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("clientoperations", async (ISender sender) =>
            {
                var result = await sender.Send(new GetClientOperationsQuery());
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.ClientOperations);
        }
    }
}