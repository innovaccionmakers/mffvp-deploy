using Closing.Application.Closing.Services.Telemetry;
using Closing.Application.PostClosing.Services.PendingTransactions;
using Closing.Application.PostClosing.Services.PortfolioCommission;
using Closing.Application.PostClosing.Services.PortfolioServices;
using Closing.Application.PostClosing.Services.TechnicalSheetEvent;
using Closing.Application.PostClosing.Services.TrustSync;
using Closing.Application.PostClosing.Services.TrustYield;
using Common.SharedKernel.Application.Helpers.Time;
using Microsoft.Extensions.Logging;

namespace Closing.Application.PostClosing.Services.Orchestation;

public class PostClosingServicesOrchestation : IPostClosingServicesOrchestation
{
    private readonly IPortfolioService _portfolioPublisher;
    private readonly ITrustYieldProcessor _trustYieldProcessor;
    private readonly IPortfolioCommissionService _commissionProcessor;
    private readonly IPendingTransactionsService _pendingTransactionHandler;
    private readonly IDataSyncPostService _dataSyncPostService;
    private readonly ITechnicalSheetPublisher _technicalSheetPublisher;
    private readonly IClosingStepTimer _stepTimer;
    private readonly ILogger<PostClosingServicesOrchestation> _logger;

    public PostClosingServicesOrchestation(
        IPortfolioService portfolioPublisher,
        ITrustYieldProcessor trustYieldProcessor,
        IPortfolioCommissionService commissionProcessor,
        IPendingTransactionsService pendingTransactionHandler,
        IDataSyncPostService dataSyncPostService,
        ITechnicalSheetPublisher technicalSheetPublisher,
        IClosingStepTimer stepTimer,
        ILogger<PostClosingServicesOrchestation> logger)
    {
        _portfolioPublisher = portfolioPublisher;
        _trustYieldProcessor = trustYieldProcessor;
        _commissionProcessor = commissionProcessor;
        _pendingTransactionHandler = pendingTransactionHandler;
        _dataSyncPostService = dataSyncPostService;
        _technicalSheetPublisher = technicalSheetPublisher;
        _stepTimer = stepTimer;
        _logger = logger;
    }

    public async Task ExecuteAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        closingDate = DateTimeConverter.ToUtcDateTime(closingDate);

        using (_stepTimer.Track("PostClosingServicesOrchestation.Execute", portfolioId, closingDate))
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                // 1,2,3: Ejecutar en paralelo 
                var valuationTask = MeasureAsync(
                    "PostClosingServicesOrchestation.PortfolioUpdate",
                    portfolioId,
                    closingDate,
                    () => _portfolioPublisher.UpdateAsync(portfolioId, closingDate, CancellationToken.None));

                var commissionTask = MeasureAsync(
                    "PostClosingServicesOrchestation.PortfolioFeeCollector",
                    portfolioId,
                    closingDate,
                    () => _commissionProcessor.ExecuteAsync(portfolioId, closingDate, CancellationToken.None));

                var syncPostTask = MeasureAsync(
                    "PostClosingServicesOrchestation.DataSyncPost",
                    portfolioId,
                    closingDate,
                    () => _dataSyncPostService.ExecuteAsync(portfolioId, closingDate, CancellationToken.None));


                await Task.WhenAll(valuationTask, commissionTask, syncPostTask);

                // 4: Rendimientos de fideicomiso
                await MeasureAsync(
                    "PostClosingServicesOrchestation.TrustYieldsGenerated",
                    portfolioId,
                    closingDate,
                    () => _trustYieldProcessor.ProcessAsync(portfolioId, closingDate, CancellationToken.None));


                // 5: Transacciones pendientes + ClosingEnd ( closingDate+1)
                await MeasureAsync(
                    "PostClosingServicesOrchestation.PendingTransactionHandler",
                    portfolioId,
                    closingDate,
                    () => _pendingTransactionHandler.HandleAsync(portfolioId, closingDate.AddDays(1), CancellationToken.None));


                // 6: Ficha técnica
                await MeasureAsync(
                    "PostClosingServicesOrchestation.TechnicalSheetPublisher",
                    portfolioId,
                    closingDate,
                    () => _technicalSheetPublisher.PublishAsync(DateOnly.FromDateTime(closingDate), CancellationToken.None));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("PostClosing cancelado para Portafolio {PortfolioId} - Fecha {ClosingDateUtc}.", portfolioId, closingDate);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error en PostClosingServicesOrchestation para Portafolio {PortfolioId} - Fecha {ClosingDateUtc}.",
                    portfolioId, closingDate);
                throw;
            }
        }
    }

    private async Task MeasureAsync(
        string stepName,
        int portfolioId,
        DateTime closingDateUtc,
        Func<Task> action)
    {
        using (_stepTimer.Track(stepName, portfolioId, closingDateUtc))
        {
            await action();
        }
    }
}
