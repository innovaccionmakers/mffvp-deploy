using Common.SharedKernel.Presentation.Endpoints;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Contributions.Integrations.ClientOperations.CreateClientOperation;

namespace Contributions.Presentation.ClientOperations
{
    internal sealed class CreateClientOperation : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            app.MapPost("clientoperations", async (Request request, ISender sender) =>
            {
                var result = await sender.Send(new CreateClientOperationCommand(
                    request.Date, 
                    request.AffiliateId, 
                    request.ObjectiveId, 
                    request.PortfolioId, 
                    request.TransactionTypeId, 
                    request.SubTransactionTypeId, 
                    request.Amount
                ));

                return result.Match(Results.Ok, ApiResults.Problem);
            })
            .WithTags(Tags.ClientOperations);
        }

        internal sealed class Request
        {
            public DateTime Date { get; init; }
            public int AffiliateId { get; init; }
            public int ObjectiveId { get; init; }
            public int PortfolioId { get; init; }
            public int TransactionTypeId { get; init; }
            public int SubTransactionTypeId { get; init; }
            public decimal Amount { get; init; }
        }
    }
}