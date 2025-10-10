using Closing.Application.Abstractions.External.Products.AccumulatedCommissions;
using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Domain.YieldDetails;
using System.Text.Json;

namespace Closing.Application.PostClosing.Services.PortfolioCommission;

/// <summary>
/// Envía (vía RPC) la actualización de comisiones acumuladas al dominio Products:
/// lee <see cref="IYieldDetailRepository"/> (cerrados), filtra "Comisión",
/// agrega por CommissionId y llama a <see cref="IUpdateAccumulatedCommissionRemote" />.
/// </summary>
public sealed class PortfolioCommissionService(
    IYieldDetailRepository repository,
    IUpdateAccumulatedCommissionRemote updateAccumulatedCommissionRemote
) : IPortfolioCommissionService
{
    public async Task ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var details = await repository
            .GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, isClosed: true, cancellationToken);

        var commissions = details
            .Where(IsCommission)
            .Where(d => d.Commissions > 0)
            .Select(d => new
            {
                CommissionId = ParseCommissionId(d.Concept),
                Value = d.Commissions
            })
            .Where(x => x.CommissionId.HasValue)
            .Select(x => new { CommissionId = x.CommissionId!.Value, x.Value });

        var aggregated = commissions
          .GroupBy(x => x.CommissionId)
          .Select(g => new
          {
              CommissionId = g.Key,
              AccumulatedValue = g.Sum(x => x.Value)
          });

        foreach (var item in aggregated)
        {
            var request = new UpdateAccumulatedCommissionRemoteRequest(
                PortfolioId: portfolioId,
                CommissionId: item.CommissionId,
                AccumulatedValue: item.AccumulatedValue,
                ClosingDateUtc: closingDate.ToUniversalTime(),
                IdempotencyKey: null,
                Origin: "Closing.PortfolioCommission",
                ExecutionId: null // <-- pásalo desde tu orquestador si lo tienes disponible
            );

            var result = await updateAccumulatedCommissionRemote.ExecuteAsync(request, cancellationToken);
            if (result.IsFailure || !result.Value.Succeeded)
            {
                // Puedes ajustar a tu Result propio o política de reintentos (Polly) si aplica
                var message = result.IsFailure
                    ? result.Error.Description
                    : (result.Value.Message ?? "RPC reported failure without message.");
                throw new InvalidOperationException(
                    $"Products RPC UpdateAccumulatedCommission failed. " +
                    $"portfolioId={portfolioId}, commissionId={item.CommissionId}, closingDate={closingDate:O}. {message}");
            }
        }
    }

    private static bool IsCommission(YieldDetail detail) =>
        detail.Source.Equals(YieldsSources.Commission, StringComparison.OrdinalIgnoreCase);

    private static int? ParseCommissionId(JsonDocument concept)
    {
        try
        {
            var root = concept.RootElement;
            if (root.TryGetProperty("EntityId", out var idProp)
                && idProp.ValueKind == JsonValueKind.String
                && int.TryParse(idProp.GetString(), out var id))
            {
                return id;
            }
        }
        catch (JsonException)
        {
            // TODO: verificar si es necesario registrar el error
        }

        return null;
    }
}