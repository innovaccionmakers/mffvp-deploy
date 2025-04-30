using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Contributions.Integrations.TrustOperations.DeleteTrustOperation;

namespace Contributions.Presentation.TrustOperations
{
    internal sealed class DeleteTrustOperation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("trustoperations/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteTrustOperationCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.TrustOperations);
        }
    }
}