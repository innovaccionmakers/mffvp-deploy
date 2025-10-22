using Closing.Integrations.Yields.Queries;
using Common.SharedKernel.Application.Rpc;
using MediatR;

namespace Closing.IntegrationEvents.Yields
{
    public sealed class GetAllAutConceptsByPortfolioIdsAndClosingDateConsumer(
        ISender sender) : IRpcHandler<GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerRequest, GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse>
    {
        public async Task<GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse> HandleAsync(GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerRequest request, CancellationToken ct)
        {
            var result = await sender.Send(new GetAllAutConceptsQuery(request.PortfolioIds, request.ClosingDate), ct);

            if (!result.IsSuccess)
                return new GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse(false, result.Error.Code, result.Error.Description, null);

            return new GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse(true, null, null, result.Value);
        }
    }
}
