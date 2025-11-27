using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Application.AutomaticConcepts.Processors;
using Accounting.Domain.PassiveTransactions;
using Accounting.Integrations.AutomaticConcepts;
using Closing.IntegrationEvents.Yields;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Application.Rpc;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AutomaticConcepts
{
    internal sealed class AutomaticConceptsHandler(
        ILoggerFactory loggerFactory,
        ISender sender,
        IRpcClient rpcClient,
        IYieldToDistributeLocator yieldToDistributeLocator,
        IYieldDetailsLocator yieldDetailsLocator,
        IPortfolioLocator portfolioLocator,
        IOperationLocator operationLocator,
        IPassiveTransactionRepository passiveTransactionRepository,
        ILogger<AutomaticConceptsHandler> logger,
        IInconsistencyHandler inconsistencyHandler) : ICommandHandler<AutomaticConceptsCommand, bool>
    {
        public async Task<Result<bool>> Handle(AutomaticConceptsCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Conceptos automáticos
                var automaticConceptsResult = await ProcessAutomaticConceptsAsync(command, cancellationToken);
                if (automaticConceptsResult.IsFailure)
                    return automaticConceptsResult;

                // Notas débito de conceptos automáticos
                var yieldsToDistributeResult = await ProcessYieldsToDistributeAsync(command, cancellationToken);
                if (yieldsToDistributeResult.IsFailure)
                    return yieldsToDistributeResult;

                return Result.Success(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al procesar los conceptos automáticos para la fecha {ProcessDate} y los Portafolios [{Portfolios}]",
                     command.ProcessDate,
                     string.Join(",", command.PortfolioIds)
                 );
                return Result.Failure<bool>(Error.Problem("Exception", "Ocurrio un error inesperado al procesar los conceptos automáticos"));
            }
        }

        private async Task<Result<bool>> ProcessAutomaticConceptsAsync(AutomaticConceptsCommand command, CancellationToken cancellationToken)
        {
            var yieldResult = await rpcClient.CallAsync<GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerRequest, GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerResponse>(
                new GetAllAutConceptsByPortfolioIdsAndClosingDateConsumerRequest(command.PortfolioIds, command.ProcessDate), cancellationToken);

            if (!yieldResult.IsValid)
                return Result.Success(true);

            var automaticConceptsProcessor = new AutomaticConceptsProcessor(
                loggerFactory.CreateLogger<AutomaticConceptsProcessor>(),
                sender,
                portfolioLocator,
                operationLocator,
                passiveTransactionRepository,
                inconsistencyHandler);

            return await automaticConceptsProcessor.ProcessAsync(yieldResult, command.ProcessDate, cancellationToken);
        }

        private async Task<Result<bool>> ProcessYieldsToDistributeAsync(AutomaticConceptsCommand command, CancellationToken cancellationToken)
        {
            var yieldsToDistributeProcessor = new YieldsToDistributeProcessor(
                loggerFactory.CreateLogger<YieldsToDistributeProcessor>(),
                sender,
                yieldToDistributeLocator,
                yieldDetailsLocator,
                portfolioLocator,
                operationLocator,
                passiveTransactionRepository,
                inconsistencyHandler);

            return await yieldsToDistributeProcessor.ProcessAsync(command.PortfolioIds, command.ProcessDate, cancellationToken);
        }
    }
}
