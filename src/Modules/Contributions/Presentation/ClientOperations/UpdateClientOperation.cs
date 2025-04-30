using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Contributions.Integrations.ClientOperations.UpdateClientOperation;

namespace Contributions.Presentation.ClientOperations
{
    internal sealed class UpdateClientOperation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPut("clientoperations/{id:guid}", async (Guid id, Request request, ISender sender) =>
            {
                var command = new UpdateClientOperationCommand(
                    id,
                    request.NewDate, 
                    request.NewAffiliateId, 
                    request.NewObjectiveId, 
                    request.NewPortfolioId, 
                    request.NewTransactionTypeId, 
                    request.NewSubTransactionTypeId, 
                    request.NewAmount
                );

                var result = await sender.Send(command);
                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.ClientOperations);
        }

        internal sealed class Request
        {
            public DateTime NewDate { get; set; }
            public int NewAffiliateId { get; set; }
            public int NewObjectiveId { get; set; }
            public int NewPortfolioId { get; set; }
            public int NewTransactionTypeId { get; set; }
            public int NewSubTransactionTypeId { get; set; }
            public decimal NewAmount { get; set; }
        }
    }
}