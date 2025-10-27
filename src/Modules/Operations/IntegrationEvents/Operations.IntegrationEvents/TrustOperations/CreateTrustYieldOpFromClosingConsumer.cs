
using Common.SharedKernel.Application.Rpc;
using MediatR;
using Microsoft.Extensions.Logging;
using Operations.Integrations.TrustOperations.Commands;

namespace Operations.IntegrationEvents.TrustOperations;

public sealed class CreateTrustYieldOpFromClosingConsumer
      : IRpcHandler<CreateTrustYieldOpFromClosingRequest, CreateTrustYieldOpFromClosingResponse>
{
    private readonly ISender mediator;
    private readonly ILogger<CreateTrustYieldOpFromClosingConsumer> logger;

    public CreateTrustYieldOpFromClosingConsumer(
        ISender mediator,
        ILogger<CreateTrustYieldOpFromClosingConsumer> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    public async Task<CreateTrustYieldOpFromClosingResponse> HandleAsync(
        CreateTrustYieldOpFromClosingRequest request,
        CancellationToken ct)
    {

        try
        {
            var command = new UpsertTrustOpFromClosingCommand(
                PortfolioId: request.PortfolioId,
                TrustYieldOperations: request.TrustYieldOperations
            );

            var result = await mediator.Send(command, ct);

            if (result.IsFailure)
            {
                logger.LogWarning("{Class} - Falló creación operaciones en Lote. Código:{Code} Mensaje:{Msg}",
                    nameof(CreateTrustYieldOpFromClosingConsumer), result.Error.Code, result.Error.Description);

                return new CreateTrustYieldOpFromClosingResponse(
                    Succeeded: false,
                    Code: result.Error.Code,
                    Message: result.Error.Description,
                    Inserted: 0,
                    Updated: 0,
                    ChangedTrustIds: Array.Empty<long>()
                );
            }   


            return new CreateTrustYieldOpFromClosingResponse(
                Succeeded: true,
                Code: null,
                Message: null,
                Inserted: result.Value.Inserted,
                Updated: result.Value.Updated,
                ChangedTrustIds: result.Value.ChangedTrustIds
            );
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("{Class} - Cancelado por token.", nameof(CreateTrustYieldOpFromClosingConsumer));
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Class} - Excepción no controlada.", nameof(CreateTrustYieldOpFromClosingConsumer));
            return new CreateTrustYieldOpFromClosingResponse(false, "OPS-UNHANDLED", ex.Message, 0, 0, Array.Empty<long>());
        }
    }
}