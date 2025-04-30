using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Contributions.Integrations.ClientOperations.DeleteClientOperation;

namespace Contributions.Presentation.ClientOperations
{
    internal sealed class DeleteClientOperation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapDelete("clientoperations/{id:guid}", async (Guid id, ISender sender) =>
            {
                var result = await sender.Send(new DeleteClientOperationCommand(id));
                return result.Match(
                    () => Results.NoContent(),
                    ApiResults.Problem
                );
            })
            .WithTags(Tags.ClientOperations);
        }
    }
}