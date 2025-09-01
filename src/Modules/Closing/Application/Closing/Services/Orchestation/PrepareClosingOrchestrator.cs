using Closing.Application.Closing.Services.Abort;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.PortfolioValuation;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustSync;
using Closing.Application.Closing.Services.Validation.Interfaces;
using Closing.Application.Closing.Services.Warnings;
using Closing.Application.PreClosing.Services.Orchestation;
using Closing.Application.PreClosing.Services.Validation;
using Closing.Integrations.Closing.RunClosing;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.Time;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Orchestration;

public class PrepareClosingOrchestrator(
    IRunSimulationValidationReader simulationValidationReader,
    IPrepareClosingBusinessValidator prepareClosingValidator,
    ITimeControlService timeControl,
    ISimulationOrchestrator simulationOrchestrator,
    IPortfolioValuationService portfolioValuationService,
    IDataSyncService dataSyncService,
    IAbortClosingService abortClosingService,
    IWarningCollector warningCollector,
    ILogger<PrepareClosingOrchestrator> logger)
    : IPrepareClosingOrchestrator
{
    public async Task<Result<PrepareClosingResult>> PrepareAsync(
        PrepareClosingCommand command,
        CancellationToken cancellationToken)
    {
        var portfolioId = command.PortfolioId;
        var closingDate = DateTimeConverter.ToUtcDateTime(command.ClosingDate);

        try
        {
            logger.LogInformation(
                "Iniciando cierre para Portafolio {PortfolioId} - Fecha {Date}",
                portfolioId, closingDate);

            // Paso 0a: Validación de Simulación + obtener IsFirstClosingDay (sin recalcular después)
            var simulationCommand = new RunSimulationCommand(portfolioId, closingDate,  true);
            var simulationValidation = await simulationValidationReader
                .ValidateAndDescribeAsync(simulationCommand, cancellationToken);

            if (simulationValidation.IsFailure)
                return Result.Failure<PrepareClosingResult>(simulationValidation.Error!);

            bool isFirstClosingDay = simulationValidation.Value.IsFirstClosingDay;

            // Paso 0b: Validación específica de PrepareClosing SOLO si NO es primer día
            if (!isFirstClosingDay)
            {
                var prepareValidation = await prepareClosingValidator
                    .ValidateAsync(command, isFirstClosingDay, cancellationToken);

                if (prepareValidation.IsFailure)
                    return Result.Failure<PrepareClosingResult>(prepareValidation.Error!);
            }


            // Paso 1: Detener el flujo transaccional (marca inicio en Redis)
            var controlResult = await timeControl.StartAsync(portfolioId, cancellationToken);
            if (controlResult.IsFailure)
                return Result.Failure<PrepareClosingResult>(controlResult.Error);

            // Paso 2: Ejecutar en paralelo:
            //  - RunSimulation (via SimulationOrchestrator)
            //  - DataSyncService

            var simulationTask = simulationOrchestrator.RunSimulationAsync(simulationCommand, cancellationToken);
            var syncTask = dataSyncService.ExecuteAsync(portfolioId, closingDate.Date, cancellationToken);
       
            await Task.WhenAll(simulationTask, syncTask);
            logger.LogInformation("TrustSync finalizado");
            if (simulationTask.Result.IsFailure)
                return Result.Failure<PrepareClosingResult>(simulationTask.Result.Error);
            if (syncTask.Result.IsFailure)
                return Result.Failure<PrepareClosingResult>(syncTask.Result.Error);

         
            // Paso 3: Calcular y persistir valoración del portafolio
            // Devuelve un PrepareClosingResult con datos financieros
            var valuationResult = await portfolioValuationService
                .CalculateAndPersistValuationAsync(portfolioId, closingDate, cancellationToken);
            if (valuationResult.IsFailure)
                return Result.Failure<PrepareClosingResult>(valuationResult.Error);

            logger.LogInformation(
                "Valoración completada para Portafolio {PortfolioId} en {Date}",
                portfolioId, closingDate);

            var warnings = warningCollector.GetAll();

            valuationResult.Value.HasWarnings = warnings.Any();
            valuationResult.Value.Warnings = warnings;

            // Paso 4: Devolver el Result generado en CalculateAndPersistValuationAsync
            return Result.Success(valuationResult.Value);
        }
        catch (Exception ex)
        {
            // En caso de error, abortar cierre y limpiar estado en Redis
            await abortClosingService.AbortAsync(portfolioId, closingDate, cancellationToken);
            logger.LogInformation(
                ex,
                "Error inesperado en PrepareClosingOrchestrator para Portafolio {PortfolioId}",
                portfolioId);

            return Result.Failure<PrepareClosingResult>(
                new Error("001", "Error inesperado durante el cierre.", ErrorType.Failure));
        }
    }
}
