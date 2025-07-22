
using Closing.Application.Closing.Services.Orchestation;
using Closing.Application.Closing.Services.TimeControl;
using Closing.Application.PreClosing.Services.Orchestation;
using Closing.Integrations.Closing.RunClosing;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Orchestration;

public class ClosingOrchestrator(
    ITimeControlService timeControl,
    ISimulationOrchestrator simulationOrchestrator,
    //IDataSyncService dataSyncService,
    //IYieldDistributionService yieldDistributionService,
    //IYieldValidationService yieldValidationService,
    //ITransactionApplierService transactionApplier,
    ILogger<ClosingOrchestrator> logger)
    : IClosingOrchestrator
{
    public async Task<Result<ClosedResult>> RunClosingAsync(RunClosingCommand command, CancellationToken cancellationToken)
    {
        var portfolioId = command.PortfolioId;
        var closingDate = command.ClosingDate;

        try
        {
            logger.LogInformation("Iniciando cierre para Portafolio {PortfolioId} - Fecha {Date}", portfolioId, closingDate);

            // Paso 1: Detención del flujo transaccional
            var controlResult = await timeControl.StartAsync(portfolioId, closingDate.Date, cancellationToken);
            if (controlResult.IsFailure)
                return Result.Failure<ClosedResult>(controlResult.Error);

            // Paso 2: Ejecutar en paralelo:
            //  - RunSimulation (via SimulationOrchestrator)
            //  - DataSyncService
            var runSimulationCommand = new RunSimulationCommand(portfolioId, closingDate, true);

            var simulationTask = simulationOrchestrator.RunSimulationAsync(runSimulationCommand, cancellationToken);
            //var syncTask = dataSyncService.ExecuteAsync(portfolioId, closingDate, cancellationToken);

            await Task.WhenAll(simulationTask);//, syncTask);

            if (simulationTask.Result.IsFailure)
                return Result.Failure<ClosedResult>(simulationTask.Result.Error);
            //if (syncTask.Result.IsFailure)
            //    return Result.Failure<ClosedResult>(syncTask.Result.Error);

            // Paso 3: Confirmación del usuario (asumida como afirmativa)

            // Paso 4: Distribución de rendimientos
            //var distributionResult = await yieldDistributionService.ExecuteAsync(portfolioId, closingDate, cancellationToken);
            //if (distributionResult.IsFailure)
            //    return Result.Failure<ClosedResult>(distributionResult.Error);

            // Paso 5: Validación de rendimientos
            //var validationResult = await yieldValidationService.ExecuteAsync(portfolioId, closingDate, cancellationToken);
            //if (validationResult.IsFailure)
            //    return Result.Failure<ClosedResult>(validationResult.Error);

            // Paso 6: Aplicación de transacciones pendientes
            //var applyResult = await transactionApplier.ExecuteAsync(portfolioId, closingDate, cancellationToken);
            //if (applyResult.IsFailure)
            //    return Result.Failure<ClosedResult>(applyResult.Error);

            // Paso 7: Reactivar flujo transaccional
            //await timeControl.EndAsync(portfolioId, cancellationToken);

            logger.LogInformation("Cierre finalizado exitosamente para portafolio {PortfolioId}", portfolioId);

            return Result.Success(new ClosedResult(portfolioId, closingDate));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error inesperado en ClosingOrchestrator para Portafolio {PortfolioId}", portfolioId);
            return Result.Failure<ClosedResult>(new Error("001","Error inesperado durante el cierre.", ErrorType.Failure));
        }
    }
}
