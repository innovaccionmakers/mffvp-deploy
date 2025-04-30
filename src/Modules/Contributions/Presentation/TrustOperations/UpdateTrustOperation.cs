using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Contributions.Integrations.TrustOperations.UpdateTrustOperation;

namespace Contributions.Presentation.TrustOperations
{
    internal sealed class UpdateTrustOperation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("trustoperations/{id:guid}", async (Guid id, Request request, ISender sender) =>
            {
                var command = new UpdateTrustOperationCommand(
                    id,
                    request.NewClientOperationId, 
                    request.NewTrustId, 
                    request.NewAmount
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.TrustOperations);
        }

        internal sealed class Request
        {
            public Guid NewClientOperationId { get; set; }
            public Guid NewTrustId { get; set; }
            public decimal NewAmount { get; set; }
        }
    }
}