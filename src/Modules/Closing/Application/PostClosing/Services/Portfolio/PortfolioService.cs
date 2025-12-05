
using Closing.Application.Abstractions.External.Products.Portfolios;
using Microsoft.Extensions.Logging;

namespace Closing.Application.PostClosing.Services.PortfolioServices;

public sealed class PortfolioService(
    IUpdatePortfolioFromClosingRemote updateRemote,
     IGetPortfolioDataRemote getRemote,
    ILogger<PortfolioService> logger)
    : IPortfolioService
{
    public async Task UpdateAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
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
            nameof(PortfolioService), resp.UpdatedCount);
    }

    public async Task<int> GetAsync(int portfolioId, CancellationToken cancellationToken)
    {
        var request = new GetPortfolioClosingDataRemoteRequest(
            PortfolioId: portfolioId
        );

        var result = await getRemote.GetAsync(request, cancellationToken);
        if (result.IsFailure) throw new InvalidOperationException($"{result.Error.Code} {result.Error.Description}");

        var resp = result.Value;
        var ok = resp.Succeeded;
        if (!ok) throw new InvalidOperationException($"GetAsync failed: {resp.Code}. {resp.Message}");

        return resp.AgileWithdrawalPercentageProtectedBalance;
    }
}
