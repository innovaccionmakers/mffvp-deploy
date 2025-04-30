using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Contributions.Integrations.TrustOperations.GetTrustOperation;

namespace Contributions.Presentation.TrustOperations
{
    internal sealed class GetTrustOperation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapGet("trustoperations/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new GetTrustOperationQuery(id));
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.TrustOperations);
        }
    }
}