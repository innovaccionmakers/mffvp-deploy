
using Common.SharedKernel.Application.Rpc;
using MediatR;
using Microsoft.Extensions.Logging;
using Operations.Integrations.TrustOperations.Commands;

namespace Operations.IntegrationEvents.TrustOperations;

public sealed class CreateTrustYieldOperationConsumer
      : IRpcHandler<CreateTrustYieldOperationRequest, CreateTrustYieldOperationResponse>
{
    private readonly ISender mediator;
    private readonly ILogger<CreateTrustYieldOperationConsumer> logger;

    public CreateTrustYieldOperationConsumer(
        ISender mediator,
        ILogger<CreateTrustYieldOperationConsumer> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    public async Task<CreateTrustYieldOperationResponse> HandleAsync(
        CreateTrustYieldOperationRequest request,
        CancellationToken ct)
    {

        try
        {
            var command = new UpsertTrustOperationCommand(
                TrustId: request.TrustId,
                PortfolioId: request.PortfolioId,
                OperationTypeId: request.OperationTypeId,
                Amount: request.YieldAmount,
                ClosingDate: request.ClosingDate,
                ProcessDate: request.ProcessDate,
                YieldRetention: request.YieldRetention,
                ClosingBalance: request.ClosingBalance
            );

            var result = await mediator.Send(command, ct);

            if (result.IsFailure)
            {
                logger.LogWarning("{Class} - Falló creación operación. Code:{Code} Msg:{Msg}",
                    nameof(CreateTrustYieldOperationConsumer), result.Error.Code, result.Error.Description);

                return new CreateTrustYieldOperationResponse(
                    Succeeded: false,
                    Code: result.Error.Code,
                    Message: result.Error.Description,
                    OperationId: null
                );
            }


            return new CreateTrustYieldOperationResponse(
                Succeeded: true,
                Code: null,
                Message: null,
                OperationId: result.Value.OperationId
            );
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("{Class} - Cancelado por token.", nameof(CreateTrustYieldOperationConsumer));
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Class} - Excepción no controlada.", nameof(CreateTrustYieldOperationConsumer));
            return new CreateTrustYieldOperationResponse(false, "OPS-UNHANDLED", ex.Message);
        }
    }
}