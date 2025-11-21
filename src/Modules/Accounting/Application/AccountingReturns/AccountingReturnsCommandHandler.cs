using Accounting.Application.Abstractions;
using Accounting.Application.Abstractions.External;
using Accounting.Domain.Constants;
using Accounting.Domain.PassiveTransactions;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingReturns;

public sealed class AccountingReturnsCommandHandler(
    ILogger<AccountingReturnsCommandHandler> logger,
    ILoggerFactory loggerFactory,
    IPassiveTransactionRepository passiveTransactionRepository,
    IYieldLocator yieldLocator,
    IYieldDetailsLocator yieldDetailsLocator,
    IPortfolioLocator portfolioLocator,
    IOperationLocator operationLocator,
    IInconsistencyHandler inconsistencyHandler,
    IMediator mediator) : ICommandHandler<AccountingReturnsCommand, bool>
{
    public async Task<Result<bool>> Handle(AccountingReturnsCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var yieldResult = await ProcessYieldsAsync(command, cancellationToken);
            if (yieldResult.IsFailure)
                return yieldResult;

            var yieldDetailResult = await ProcessYieldsDetailsAsync(command, cancellationToken);
            if (yieldDetailResult.IsFailure)
                return yieldDetailResult;

            return Result.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error al procesar los rendimientos contables para la fecha {ProcessDate} y los Portafolios [{Portfolios}]",
                command.ProcessDate,
                string.Join(",", command.PortfolioIds)
            );
            return Result.Failure<bool>(Error.Problem("Exception", "Ocurrió un error inesperado al procesar los rendimientos contables"));
        }
    }

    private async Task<Result<bool>> ProcessYieldsAsync(AccountingReturnsCommand command, CancellationToken cancellationToken)
    {
        var yieldProcessor = new YieldProcessor(
            loggerFactory.CreateLogger<YieldProcessor>(),
            passiveTransactionRepository,
            yieldLocator,
            portfolioLocator,
            operationLocator,
            inconsistencyHandler,
            mediator);

        return await yieldProcessor.ProcessAsync(command.PortfolioIds, command.ProcessDate, cancellationToken);
    }

    private async Task<Result<bool>> ProcessYieldsDetailsAsync(AccountingReturnsCommand command, CancellationToken cancellationToken)
    {
        var yieldDetailProcessor = new YieldDetailProcessor(
            loggerFactory.CreateLogger<YieldDetailProcessor>(),
            passiveTransactionRepository,
            yieldDetailsLocator,
            portfolioLocator,
            operationLocator,
            inconsistencyHandler,
            mediator);

        return await yieldDetailProcessor.ProcessAsync(command.PortfolioIds, command.ProcessDate, cancellationToken);
    }
}
