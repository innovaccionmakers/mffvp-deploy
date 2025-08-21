using Closing.Application.PostClosing.Services.PendingTransactionHandler;
using Closing.Application.PostClosing.Services.PortfolioCommissionEvent;
using Closing.Application.PostClosing.Services.PortfolioUpdateEvent;
using Closing.Application.PostClosing.Services.TrustSync;
using Closing.Application.PostClosing.Services.TrustYieldEvent;
using Common.SharedKernel.Application.Helpers.General;

namespace Closing.Application.PostClosing.Services.Orchestation;

public class PostClosingEventsOrchestation : IPostClosingEventsOrchestation
{
    private readonly IPortfolioUpdatePublisher _portfolioPublisher;
    private readonly ITrustYieldPublisher _trustYieldPublisher;
    private readonly IPortfolioCommissionPublisher _commissionPublisher;
    private readonly IPendingTransactionHandler _pendingTransactionHandler;
    private readonly IDataSyncPostService _dataSyncPostService;

    public PostClosingEventsOrchestation(
        IPortfolioUpdatePublisher portfolioPublisher,
        ITrustYieldPublisher trustYieldPublisher,
        IPortfolioCommissionPublisher commissionPublisher,
        IPendingTransactionHandler pendingTransactionHandler,
        IDataSyncPostService dataSyncPostService
        )
    {
        _portfolioPublisher = portfolioPublisher;
        _trustYieldPublisher = trustYieldPublisher;
        _commissionPublisher = commissionPublisher;
        _pendingTransactionHandler = pendingTransactionHandler;
        _dataSyncPostService = dataSyncPostService;
    }

    public async Task ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        closingDate = DateTimeConverter.ToUtcDateTime(closingDate);
        // 1. Publicar eventos de actualización del portafolio
        var valuationTask = _portfolioPublisher.PublishAsync(portfolioId, closingDate, cancellationToken);

        // 2. Publicar eventos de comisión del portafolio
        var commissionTask = _commissionPublisher.PublishAsync(portfolioId, closingDate, cancellationToken);

        // 3. Publicar eventos de sincronización de datos
        var syncPostTask = _dataSyncPostService.ExecuteAsync(portfolioId, closingDate, cancellationToken);

        // Ejecutar las tres tareas en paralelo
        await Task.WhenAll(valuationTask, commissionTask, syncPostTask);

        //// sincrono
        //// 1. Publicar eventos de actualización del portafolio
        //await _portfolioPublisher.PublishAsync(portfolioId, closingDate, cancellationToken);

        //// 2. Publicar eventos de comisión del portafolio
        //await _commissionPublisher.PublishAsync(portfolioId, closingDate, cancellationToken);

        //// 3. Publicar eventos de sincronización de datos
        //await _dataSyncPostService.ExecuteAsync(portfolioId, closingDate, cancellationToken);

        // 4. Publicar eventos de rendimientos de fideicomiso
        await _trustYieldPublisher.PublishAsync(portfolioId, closingDate, cancellationToken);

        // 5. Manejar transacciones pendientes y enviar evento de paso ClosingEnd
        await _pendingTransactionHandler.HandleAsync(portfolioId, closingDate, cancellationToken);
    }
}