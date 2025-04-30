using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Contributions.Integrations.ClientOperations.GetClientOperation;

namespace Contributions.Presentation.ClientOperations
{
    internal sealed class GetClientOperation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("clientoperations/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetClientOperationQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.ClientOperations);
        }
    }
}