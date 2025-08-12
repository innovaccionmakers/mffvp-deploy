using Closing.Application.PostClosing.Services.PendingTransactionHandler;
using Closing.Application.PostClosing.Services.PortfolioCommissionEvent;
using Closing.Application.PostClosing.Services.PortfolioUpdateEvent;
using Closing.Application.PostClosing.Services.TrustYieldEvent;

namespace Closing.Application.PostClosing.Services.Orchestation;

public class PostClosingEventsOrchestation : IPostClosingEventsOrchestation
{
    private readonly IPortfolioUpdatePublisher _valuationPublisher;
    private readonly ITrustYieldPublisher _trustYieldPublisher;
    private readonly IPortfolioCommissionPublisher _commissionPublisher;
    private readonly IPendingTransactionHandler _pendingTransactionHandler;

    public PostClosingEventsOrchestation(
        IPortfolioUpdatePublisher valuationPublisher,
        ITrustYieldPublisher trustYieldPublisher,
        IPortfolioCommissionPublisher commissionPublisher,
        IPendingTransactionHandler pendingTransactionHandler
        )
    {
        _valuationPublisher = valuationPublisher;
        _trustYieldPublisher = trustYieldPublisher;
        _commissionPublisher = commissionPublisher;
        _pendingTransactionHandler = pendingTransactionHandler;
    }

    public async Task ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        //// 1. Publicar eventos de valoración del portafolio
        //var valuationTask = _valuationPublisher.PublishAsync(portfolioId, closingDate, cancellationToken);

        //// 2. Publicar eventos de rendimientos de fideicomiso
        //var trustReturnsTask = _trustYieldPublisher.PublishAsync(portfolioId, closingDate, cancellationToken);

        //// 3. Publicar eventos de comisión del portafolio
        //var commissionTask = _commissionPublisher.PublishAsync(portfolioId, closingDate, cancellationToken);

        //// Ejecutar las tres tareas en paralelo
        //await Task.WhenAll(valuationTask, trustReturnsTask, commissionTask);

        // 1. Publicar eventos de valoración del portafolio
        await _valuationPublisher.PublishAsync(portfolioId, closingDate, cancellationToken);

        // 2. Publicar eventos de rendimientos de fideicomiso
        await _trustYieldPublisher.PublishAsync(portfolioId, closingDate, cancellationToken);

        // 3. Publicar eventos de comisión del portafolio
        await _commissionPublisher.PublishAsync(portfolioId, closingDate, cancellationToken);

        // 4. Manejar transacciones pendientes y enviar evento de paso ClosingEnd
        await _pendingTransactionHandler.HandleAsync(portfolioId, closingDate, cancellationToken);
    }
}