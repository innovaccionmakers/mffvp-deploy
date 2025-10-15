using Accounting.Domain.Constants;
using Accounting.IntegrationEvents.AccountingProcess;
using Accounting.Integrations.AccountingAssistants.Commands;
using Accounting.Integrations.AccountingConcepts;
using Accounting.Integrations.AccountingFees;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.AccountingReturns;
using Accounting.Integrations.AccountProcess;
using Common.SharedKernel.Application.Caching.Closing.Interfaces;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using MediatR;

namespace Accounting.Application.AccountProcess
{
    internal sealed class AccountProcessHandler(
        ISender sender,
        IClosingExecutionStore closingValidator, IEventBus eventBus) : ICommandHandler<AccountProcessCommand, string>
    {
        public async Task<Result<string>> Handle(AccountProcessCommand command, CancellationToken cancellationToken)
        {            
            var processDate = command.ProcessDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            
            var isActive = await closingValidator.IsClosingActiveAsync(cancellationToken);
            if (isActive)
                return Result.Failure<string>(new Error("0001", "Existe un proceso de cierre activo.", ErrorType.Validation));

            var deleteCommand = new DeleteAccountingAssistantsCommand();
            var deleteResult = await sender.Send(deleteCommand, cancellationToken);
            if (deleteResult.IsFailure)
                return Result.Failure<string>(deleteResult.Error);

            var processId = Guid.NewGuid();

            var accountingFeesCommand = new AccountingFeesCommand(command.PortfolioIds, processDate);
            var accountingReturnsCommand = new AccountingReturnsCommand(command.PortfolioIds, processDate);
            var acountingOperationsCommand = new AccountingOperationsCommand(command.PortfolioIds, processDate);
            var accountingConceptsCommand = new AccountingConceptsCommand(command.PortfolioIds, processDate);

            _ = Task.Run(async () => await ExecuteAccountingOperationAsync(ProcessTypes.AccountingFees, accountingFeesCommand, processId, processDate, command.PortfolioIds, cancellationToken), cancellationToken);
            _ = Task.Run(async () => await ExecuteAccountingOperationAsync(ProcessTypes.AccountingReturns, accountingReturnsCommand, processId, processDate, command.PortfolioIds, cancellationToken), cancellationToken);
            _ = Task.Run(async () => await ExecuteAccountingOperationAsync(ProcessTypes.AccountingOperations, acountingOperationsCommand, processId, processDate, command.PortfolioIds, cancellationToken), cancellationToken);
            _ = Task.Run(async () => await ExecuteAccountingOperationAsync(ProcessTypes.AccountingConcepts, accountingConceptsCommand, processId, processDate, command.PortfolioIds, cancellationToken), cancellationToken);

            return Result.Success(string.Empty, "Se está generando la información del proceso contable. Será notificado cuando finalice.");
        }



        private async Task ExecuteAccountingOperationAsync<T>(
            string operationType,
            T command,
            Guid processId,
            DateTime processDate,
            IEnumerable<int> portfolioIds,
            CancellationToken cancellationToken) where T : ICommand<bool>
        {
            try
            {
                var result = await sender.Send(command, cancellationToken);

                var success = result.IsSuccess;
                var message = success
                    ? $"{operationType} procesado exitosamente"
                    : $"{result.Error?.Description ?? "falló durante el procesamiento"}";

                await PublishProcessCompletedAsync(
                    operationType,
                    success,
                    success ? null : message,
                    processId,
                    processDate,
                    portfolioIds,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                await PublishProcessCompletedAsync(
                    operationType,
                    false,
                    $"Excepción durante el procesamiento: {ex.Message}",
                    processId,
                    processDate,
                    portfolioIds,
                    cancellationToken);
            }
        }

        private async Task PublishProcessCompletedAsync(
            string processType,
            bool isSuccess,
            string? errorMessage,
            Guid processId,
            DateTime processDate,
            IEnumerable<int> portfolioIds,
            CancellationToken cancellationToken)
        {
            var evt = new AccountingProcessCompletedIntegrationEvent(
                processType,
                isSuccess,
                errorMessage,
                processId,
                processDate,
                portfolioIds);

            await eventBus.PublishAsync(evt, cancellationToken);
        }
    }
}
