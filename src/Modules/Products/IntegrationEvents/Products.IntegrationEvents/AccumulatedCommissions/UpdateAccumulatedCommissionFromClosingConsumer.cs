
using Common.SharedKernel.Application.Rpc;
using MediatR;
using Microsoft.Extensions.Logging;
using Products.Integrations.AccumulatedCommissions.Commands;

namespace Products.IntegrationEvents.AccumulatedCommissions;

public sealed class UpdateAccumulatedCommissionFromClosingConsumer
        : IRpcHandler<UpdateAccumulatedCommissionFromClosingRequest, UpdateAccumulatedCommissionFromClosingResponse>
{
    private readonly ISender mediator;
    private readonly ILogger<UpdateAccumulatedCommissionFromClosingConsumer> logger;

    public UpdateAccumulatedCommissionFromClosingConsumer(
        ISender mediator,
        ILogger<UpdateAccumulatedCommissionFromClosingConsumer> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    public async Task<UpdateAccumulatedCommissionFromClosingResponse> HandleAsync(
        UpdateAccumulatedCommissionFromClosingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation(
                "[{Class}] UpdateAccumulatedFromClosing → Portfolio={PortfolioId} Commission={CommissionId} Amount={Amount} Date={Date} IdemKey={Key} Origin={Origin} ExecId={ExecId}",
                nameof(UpdateAccumulatedCommissionFromClosingConsumer),
                request.PortfolioId, request.CommissionId, request.AccumulatedValue,
                request.ClosingDate, request.IdempotencyKey, request.Origin, request.ExecutionId);

            await mediator.Send(
                new UpsertAccumulatedCommissionCommand(
                    request.PortfolioId,
                    request.CommissionId,
                    request.AccumulatedValue,
                    request.ClosingDate),
                cancellationToken);

            // Si tu command retorna métricas o detecta NoChange, puedes reflejarlo aquí.
            return new UpdateAccumulatedCommissionFromClosingResponse(
                Succeeded: true,
                Status: "Updated",     // o "NoChange" si tu command lo determina
                Code: "OK",
                Message: "Comisión acumulada actualizada desde Closing."
            );
        }
        catch (OperationCanceledException)
        {
            return new UpdateAccumulatedCommissionFromClosingResponse(
                Succeeded: false,
                Status: "Error",
                Code: "PROD-CANCELED",
                Message: "Operación cancelada."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[{Class}] Error al actualizar comisión acumulada. Portfolio={PortfolioId} Commission={CommissionId} Date={Date}",
                nameof(UpdateAccumulatedCommissionFromClosingConsumer),
                request.PortfolioId, request.CommissionId, request.ClosingDate);

            return new UpdateAccumulatedCommissionFromClosingResponse(
                Succeeded: false,
                Status: "Error",
                Code: "PROD-UNHANDLED",
                Message: ex.Message
            );
        }
    }
}