using Accounting.Domain.Constants;
using Accounting.Integrations.AccountingConcepts;
using Accounting.Integrations.AccountingFees;
using Accounting.Integrations.AccountingOperations;
using Accounting.Integrations.AccountingReturns;
using Accounting.Integrations.AutomaticConcepts;
using Common.SharedKernel.Application.EventBus;
using Common.SharedKernel.Application.Messaging;
using DotNetCore.CAP;
using MediatR;

namespace Accounting.IntegrationEvents.AccountingProcess;

public sealed class AccountingOperationRequestedSubscriber(ISender sender, IEventBus eventBus) : ICapSubscribe
{
    [CapSubscribe(nameof(AccountingOperationRequestedIntegrationEvent))]
    public async Task HandleAsync(AccountingOperationRequestedIntegrationEvent message, CancellationToken cancellationToken)
    {
        try
        {
            ICommand<bool> command = message.ProcessType switch
            {
                ProcessTypes.AccountingFees => new AccountingFeesCommand(message.PortfolioIds, message.ProcessDate),
                ProcessTypes.AccountingReturns => new AccountingReturnsCommand(message.PortfolioIds, message.ProcessDate),
                ProcessTypes.AccountingOperations => new AccountingOperationsCommand(message.PortfolioIds, message.ProcessDate),
                ProcessTypes.AccountingConcepts => new AccountingConceptsCommand(message.PortfolioIds, message.ProcessDate),
                ProcessTypes.AutomaticConcepts => new AutomaticConceptsCommand(message.PortfolioIds, message.ProcessDate),
                _ => throw new ArgumentException($"Tipo de proceso no válido: {message.ProcessType}", nameof(message.ProcessType))
            };

            var result = await sender.Send(command, cancellationToken);

            var success = result.IsSuccess;
            var errorMessage = success
                ? null
                : result.Error?.Description ?? "falló durante el procesamiento";

            var completedEvent = new AccountingProcessCompletedIntegrationEvent(
                message.User,
                message.Email,
                message.ProcessType,
                success,
                errorMessage,
                message.ProcessId,
                message.StartDate,
                message.ProcessDate,
                message.PortfolioIds);

            await eventBus.PublishAsync(completedEvent, cancellationToken);
        }
        catch (Exception ex)
        {
            var completedEvent = new AccountingProcessCompletedIntegrationEvent(
                message.User,
                message.Email,
                message.ProcessType,
                false,
                $"Excepción durante el procesamiento: {ex.Message}",
                message.ProcessId,
                message.StartDate,
                message.ProcessDate,
                message.PortfolioIds);

            await eventBus.PublishAsync(completedEvent, cancellationToken);
        }
    }
}

