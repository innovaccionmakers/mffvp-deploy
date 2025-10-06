

using Closing.Application.PostClosing.Services.PendingTransactions;
using Closing.Application.PostClosing.Services.PortfolioCommission;
using Closing.Application.PostClosing.Services.PortfolioUpdate;
using Closing.Application.PostClosing.Services.TechnicalSheetEvent;
using Closing.Application.PostClosing.Services.TrustSync;
using Closing.Application.PostClosing.Services.TrustYield;
using Common.SharedKernel.Application.Helpers.Time;

namespace Closing.Application.PostClosing.Services.Orchestation;

public class PostClosingServicesOrchestation : IPostClosingServicesOrchestation
{
    private readonly IPortfolioUpdateService _portfolioPublisher;
    private readonly ITrustYieldProcessor _trustYieldProcessor;
    private readonly IPortfolioCommissionService _commissionProcessor;
    private readonly IPendingTransactionsService _pendingTransactionHandler;
    private readonly IDataSyncPostService _dataSyncPostService;
    private readonly ITechnicalSheetPublisher _technicalSheetPublisher;

    public PostClosingServicesOrchestation(
        IPortfolioUpdateService portfolioPublisher,
        ITrustYieldProcessor trustYieldProcessor,
        IPortfolioCommissionService commissionProcessor,
        IPendingTransactionsService pendingTransactionHandler,
        IDataSyncPostService dataSyncPostService,
        ITechnicalSheetPublisher technicalSheetPublisher
        )
    {
        _portfolioPublisher = portfolioPublisher;
        _trustYieldProcessor = trustYieldProcessor;
        _commissionProcessor = commissionProcessor;
        _pendingTransactionHandler = pendingTransactionHandler;
        _dataSyncPostService = dataSyncPostService;
        _technicalSheetPublisher = technicalSheetPublisher;
    }

    public async Task ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        closingDate = DateTimeConverter.ToUtcDateTime(closingDate);
        // 1. Publicar eventos de actualización del portafolio
        var valuationTask = _portfolioPublisher.ExecuteAsync(portfolioId, closingDate, cancellationToken);

        // 2. Publicar eventos de comisión del portafolio
        var commissionTask = _commissionProcessor.ExecuteAsync(portfolioId, closingDate, cancellationToken);

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
        await _trustYieldProcessor.ProcessAsync(portfolioId, closingDate, cancellationToken);

        // 5. Manejar transacciones pendientes y enviar evento de paso ClosingEnd
        await _pendingTransactionHandler.HandleAsync(portfolioId, closingDate.AddDays(1), cancellationToken);

        //4. Publicar eventos de generacion ficha tecnica
        await _technicalSheetPublisher.PublishAsync(DateOnly.FromDateTime(closingDate), cancellationToken);

    }
}
