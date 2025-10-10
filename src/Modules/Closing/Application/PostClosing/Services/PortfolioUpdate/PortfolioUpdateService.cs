
using Closing.Application.Abstractions.External.Products.Portfolios;
using Microsoft.Extensions.Logging;

namespace Closing.Application.PostClosing.Services.PortfolioUpdate;

public sealed class PortfolioUpdateService(
    IUpdatePortfolioFromClosingRemote updateRemote,
    ILogger<PortfolioUpdateService> logger)
    : IPortfolioUpdateService
{
    public async Task ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var idempotencyKey = $"portfolio-upd:{portfolioId}:{closingDate:yyyyMMdd}";
        var request = new UpdatePortfolioFromClosingRemoteRequest(
            PortfolioId: portfolioId,
            ClosingDateUtc: closingDate,
            IdempotencyKey: idempotencyKey,
            Origin: "Closing",              
            ExecutionId: null
        );

        var result = await updateRemote.ExecuteAsync(request, cancellationToken);
        if (result.IsFailure) throw new InvalidOperationException($"{result.Error.Code} {result.Error.Description}");

        var resp = result.Value;
        var ok = resp.Succeeded && (resp.Status == "Updated" || resp.Status == "NoChange");
        if (!ok) throw new InvalidOperationException($"UpdatePortfolioFromClosing failed: {resp.Status}. {resp.Message}");

        logger.LogInformation("[{Class}] Portafolio actualizado. UpdatedCount={Count}",
            nameof(PortfolioUpdateService), resp.UpdatedCount);
    }
}
