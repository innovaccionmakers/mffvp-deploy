using Accounting.Integrations.AccountValidator;
using DotNetCore.CAP;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Accounting.IntegrationEvents.AccountingInconsistencies;

/// <summary>
/// Suscriptor de eventos para AccountingServiceResultIntegrationEvent
/// Se encarga de validar el estado de los servicios contables y manejar inconsistencias
/// </summary>
public sealed class AccountingServiceResultEventSubscriber(
    ISender mediator,
    ILogger<AccountingServiceResultEventSubscriber> logger) : ICapSubscribe
{
    [CapSubscribe(nameof(AccountingServiceResultIntegrationEvent))]
    public async Task HandleAsync(AccountingServiceResultIntegrationEvent message,
                                [FromCap] CapHeader headers,
                                CancellationToken cancellationToken)
    {
        var msgId = headers.TryGetValue("cap-msg-id", out var id) ? id : "-";
        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["CapMessageId"] = msgId,
            ["Topic"] = nameof(AccountingServiceResultIntegrationEvent),
            ["ProcessType"] = message.ProcessType,
            ["Success"] = message.Success,
            ["Message"] = message.Message
        }))
        {
            try
            {
                logger.LogInformation("Procesando resultado del servicio contable: {ProcessType} - Success: {Success}",
                    message.ProcessType, message.Success);

                var result = await mediator.Send(new AccountValidatorCommand(message.ProcessDate, ""), cancellationToken);

                if (result.IsFailure)
                {
                    logger.LogWarning("Error en la validación de cuentas: {Error}", result.Error);
                    return;
                }

                logger.LogInformation("Validación de cuentas completada exitosamente");
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("CAP Cancelado antes o durante mediator.Send");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CAP Excepción alrededor de mediator.Send");
                throw;
            }
        }
    }
}
