
using Common.SharedKernel.Application.Rpc;
using MediatR;
using Microsoft.Extensions.Logging;
using Products.Integrations.Portfolios.Commands;

namespace Products.IntegrationEvents.Portfolio.UpdateFromClosing;

public sealed class UpdatePortfolioFromClosingConsumer
       : IRpcHandler<UpdatePortfolioFromClosingRequest, UpdatePortfolioFromClosingResponse>
{
    private readonly ISender mediator;
    private readonly ILogger<UpdatePortfolioFromClosingConsumer> logger;

    public UpdatePortfolioFromClosingConsumer(ISender mediator, ILogger<UpdatePortfolioFromClosingConsumer> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    public async Task<UpdatePortfolioFromClosingResponse> HandleAsync(
        UpdatePortfolioFromClosingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await mediator.Send(
                new UpdatePortfolioFromClosingCommand(request.PortfolioId, request.ClosingDate),
                cancellationToken);

            return new UpdatePortfolioFromClosingResponse(
                Succeeded: true,
                Status: "Updated",    
                UpdatedCount: 1,
                Code: "OK",
                Message: "Portafolio actualizado desde Closing."
            );
        }
        catch (OperationCanceledException)
        {
            return new UpdatePortfolioFromClosingResponse(
                Succeeded: false,
                Status: "Error",
                UpdatedCount: 0,
                Code: "PROD-CANCELED",
                Message: "Operación cancelada."
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "[{Class}] Error al actualizar portafolio desde Closing. Portfolio={PortfolioId} Date={Date}",
                nameof(UpdatePortfolioFromClosingConsumer), request.PortfolioId, request.ClosingDate);

            return new UpdatePortfolioFromClosingResponse(
                Succeeded: false,
                Status: "Error",
                UpdatedCount: 0,
                Code: "PROD-UNHANDLED",
                Message: ex.Message
            );
        }
    }
}
