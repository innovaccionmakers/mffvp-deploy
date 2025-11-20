using Common.SharedKernel.Application.Rpc;
using MediatR;
using Microsoft.Extensions.Logging;
using Trusts.Integrations.TrustYields.Commands; 
using Trusts.Domain.Trusts.TrustYield;   

namespace Trusts.IntegrationEvents.TrustYields;

public sealed class UpdateTrustFromYieldConsumer
  : IRpcHandler<UpdateTrustFromYieldRequest, UpdateTrustFromYieldResponse>
{
    private readonly ISender mediator;
    private readonly ILogger<UpdateTrustFromYieldConsumer> logger;

    public UpdateTrustFromYieldConsumer(
        ISender mediator,
        ILogger<UpdateTrustFromYieldConsumer> logger)
    {
        this.mediator = mediator;
        this.logger = logger;
    }

    public async Task<UpdateTrustFromYieldResponse> HandleAsync(
        UpdateTrustFromYieldRequest request,
        CancellationToken ct)
    {
        try
        {
            var rows = (request.Rows ?? Array.Empty<ApplyYieldRowDto>())
                .Select(r => new ApplyYieldRow(
                    TrustId: r.TrustId,
                    YieldAmount: r.YieldAmount,
                    YieldRetentionRate: r.YieldRetention,
                    ClosingBalance: r.ClosingBalance))
                .ToList();

            var command = new UpdateTrustFromYieldCommand(
                Rows: rows,
                BatchIndex: request.BatchIndex
            );

            var result = await mediator.Send(command, ct);

            if (result.IsFailure)
            {
                return new UpdateTrustFromYieldResponse(
                    Succeeded: false,
                    BatchIndex: request.BatchIndex,
                    Updated: 0,
                    MissingTrustIds: Array.Empty<long>(),
                    ValidationMismatchTrustIds: Array.Empty<long>(),
                    Code: result.Error.Code,
                    Message: result.Error.Description
                );
            }

            var batch = result.Value; // ApplyYieldBulkBatchResult

            return new UpdateTrustFromYieldResponse(
                Succeeded: true,
                BatchIndex: batch.BatchIndex,
                Updated: batch.Updated,
                MissingTrustIds: batch.MissingTrustIds,
                ValidationMismatchTrustIds: batch.ValidationMismatchTrustIds
            );
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("{Handler} - Cancelado por token. Lote {BatchIndex}, Portafolio {PortfolioId}, Fecha {ClosingDate}",
                nameof(UpdateTrustFromYieldConsumer), request.BatchIndex, request.PortfolioId, request.ClosingDate);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{Handler} - Excepción no controlada. Lote {BatchIndex}, Portafolio {PortfolioId}, Fecha {ClosingDate}",
                nameof(UpdateTrustFromYieldConsumer), request.BatchIndex, request.PortfolioId, request.ClosingDate);

            return new UpdateTrustFromYieldResponse(
                Succeeded: false,
                BatchIndex: request.BatchIndex,
                Updated: 0,
                MissingTrustIds: Array.Empty<long>(),
                ValidationMismatchTrustIds: Array.Empty<long>(),
                Code: "TRU-UNHANDLED",
                Message: ex.Message
            );
        }
    }
}