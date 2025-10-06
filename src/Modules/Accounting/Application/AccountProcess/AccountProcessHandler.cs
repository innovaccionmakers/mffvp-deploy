using Accounting.Domain.Constants;
using Accounting.IntegrationEvents.AccountingInconsistencies;
using Accounting.Integrations.AccountingConcepts;
using Accounting.Integrations.AccountingFees;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.AccountingReturns;
using Accounting.Integrations.AccountProcess;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using MediatR;

namespace Accounting.Application.AccountProcess
{
    internal sealed class AccountProcessHandler(
        ISender sender,
        IClosingExecutionStore closingValidator,
        IEventBus eventBus) : ICommandHandler<AccountProcessCommand, string>
    {
        public async Task<Result<string>> Handle(AccountProcessCommand command, CancellationToken cancellationToken)
        {
            var isActive = await closingValidator.IsClosingActiveAsync(cancellationToken);
            if (isActive)
                return Result.Failure<string>(new Error("0001", "Existe un proceso de cierre activo.", ErrorType.Validation));

            var accountingFeesCommand = new AccountingFeesCommand(command.PortfolioIds, command.ProcessDate);
            var accountingReturnsCommand = new AccountingReturnsCommand(command.PortfolioIds, command.ProcessDate);
            var acountingOperationsCommand = new AccountingOperationsCommand(command.PortfolioIds, command.ProcessDate);
            var accountingConceptsCommand = new AccountingConceptsCommand(command.PortfolioIds, command.ProcessDate);

            _ = Task.Run(async () => await ExecuteAccountingOperationAsync(ProcessTypes.AccountingFees, accountingFeesCommand, cancellationToken), cancellationToken);
            _ = Task.Run(async () => await ExecuteAccountingOperationAsync(ProcessTypes.AccountingReturns, accountingReturnsCommand, cancellationToken), cancellationToken);
            _ = Task.Run(async () => await ExecuteAccountingOperationAsync(ProcessTypes.AccountingOperations, acountingOperationsCommand, cancellationToken), cancellationToken);
            _ = Task.Run(async () => await ExecuteAccountingOperationAsync(ProcessTypes.AccountingConcepts, accountingConceptsCommand, cancellationToken), cancellationToken);

            return Result.Success<string>("Proceso de contabilidad iniciado exitosamente en segundo plano.");
        }

        private async Task ExecuteAccountingOperationAsync<T>(string operationType, T command, CancellationToken cancellationToken) where T : ICommand<bool>
        {
            var result = await sender.Send(command, cancellationToken);

            var success = result.Value;
            var message = success
                ? $"{operationType} procesado exitosamente"
                : $"{operationType} falló durante el procesamiento";

            var resultEvent = new AccountingServiceResultIntegrationEvent(success, operationType, message);
            await eventBus.PublishAsync(resultEvent, cancellationToken);         
        }
    }
}
