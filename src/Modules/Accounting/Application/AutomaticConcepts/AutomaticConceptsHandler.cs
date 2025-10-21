using Accounting.Integrations.AutomaticConcepts;
using Closing.IntegrationEvents.Yields;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AutomaticConcepts
{
    internal sealed class AutomaticConceptsHandler(
        IRpcClient rpcClient,
        ISender sender,
        ILogger<AutomaticConceptsHandler> logger) : ICommandHandler<AutomaticConceptsCommand, bool>
    {
        public async Task<Result<bool>> Handle(AutomaticConceptsCommand command, CancellationToken cancellationToken)
        {
            var yield = await rpcClient.CallAsync<GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerRequest, GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse>(
                                                            new GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerRequest(command.PortfolioIds, command.ProcessDate), cancellationToken);
            return Result<bool>.Success(true);
        }
    }
}
