using Accounting.Integrations.AccountingValidator;
using DotNetCore.CAP;
using MediatR;

namespace Accounting.IntegrationEvents.AccountingProcess;

public sealed class AccountingProcessCompletedIntegrationSuscriber(ISender sender) : ICapSubscribe
{

    [CapSubscribe(nameof(AccountingProcessCompletedIntegrationEvent))]
    public async Task HandleAsync(AccountingProcessCompletedIntegrationEvent message, CancellationToken cancellationToken)
    {
        await sender.Send(new AccountingValidatorCommand(message.ProcessType,
                                                         message.IsSuccess,
                                                         message.ErrorMessage,
                                                         message.ProcessId,
                                                         message.ProcessDate,
                                                         message.PortfolioIds), cancellationToken);
    }
}
