using Closing.Application.PreClosing.Services.Yield.Constants;
using Closing.Domain.YieldDetails;
using Closing.IntegrationEvents.PostClosing;
using Common.SharedKernel.Application.EventBus;
using System.Text.Json;

namespace Closing.Application.PostClosing.Services.PortfolioCommissionEvent;

/// <summary>
/// Publica eventos de comisiones procesadas al cierre: lee detalles cerrados con
/// <see cref="IYieldDetailRepository"/>, identifica registros de tipo "Comisión"
/// y emite <see cref="CommissionProcessedIntegrationEvent"/> vía <see cref="IEventBus" />
/// para notificar al dominio Product y actualizar comisiones acumuladas.
/// </summary>

public sealed class PortfolioCommissionPublisher(
    IYieldDetailRepository repository,
    IEventBus eventBus
) : IPortfolioCommissionPublisher
{
    public async Task PublishAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var details = await repository
            .GetReadOnlyByPortfolioAndDateAsync(portfolioId, closingDate, isClosed: true, cancellationToken);

        foreach (var detail in details)
        {
            if (!IsCommission(detail) || detail.Commissions <= 0)
                continue;

            var commissionId = ParseCommissionId(detail.Concept);
            if (commissionId is null)
                continue;

            var @event = new CommissionProcessedIntegrationEvent(
                portfolioId: detail.PortfolioId,
                commissionId: commissionId.Value,
                accumulatedValue: detail.Commissions,
                closingDate: detail.ClosingDate
            );

            await eventBus.PublishAsync(@event, cancellationToken);
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