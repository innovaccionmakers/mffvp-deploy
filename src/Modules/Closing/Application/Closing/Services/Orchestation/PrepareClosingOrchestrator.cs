using Closing.Application.Closing.Services.Abort;
using Closing.Application.Closing.Services.Orchestation.Interfaces;
using Closing.Application.Closing.Services.PortfolioValuation;
using Closing.Application.Closing.Services.TimeControl.Interrfaces;
using Closing.Application.Closing.Services.TrustSync;
using Closing.Application.PreClosing.Services.Orchestation;
using Closing.Application.PreClosing.Services.Validation;
using Closing.Integrations.Closing.RunClosing;
using Closing.Integrations.PreClosing.RunSimulation;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Domain;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Closing.Application.Closing.Services.Orchestration;

public class PrepareClosingOrchestrator(
    IBusinessValidator<RunSimulationCommand> businessValidator,
    ITimeControlService timeControl,
    ISimulationOrchestrator simulationOrchestrator,
    IPortfolioValuationService portfolioValuationService,
    IDataSyncService dataSyncService,
    IAbortClosingService abortClosingService,
    ILogger<PrepareClosingOrchestrator> logger)
    : IPrepareClosingOrchestrator
{
    public async Task<Result<ClosedResult>> PrepareAsync(RunClosingCommand command, CancellationToken cancellationToken)
    {

        var portfolioId = command.PortfolioId;
        var closingDate = DateTimeConverter.ToUtcDateTime(command.ClosingDate);
        try
        {
            logger.LogInformation("Iniciando cierre para Portafolio {PortfolioId} - Fecha {Date}", portfolioId, closingDate);
            //Paso 0: Validación de parámetros
            var runSimulationCommand = new RunSimulationCommand(portfolioId, closingDate, true);

            var validationResult = await ValidateBusinessRulesAsync(runSimulationCommand, cancellationToken);
            if (validationResult.IsFailure)
                return Result.Failure<ClosedResult>(validationResult.Error!);

            // Paso 1: Detención del flujo transaccional
            var controlResult = await timeControl.StartAsync(portfolioId, cancellationToken);
            if (controlResult.IsFailure)
                return Result.Failure<ClosedResult>(controlResult.Error);

            // Paso 2: Ejecutar en paralelo:
            //  - RunSimulation (via SimulationOrchestrator)
            //  - DataSyncService

            var simulationTask = simulationOrchestrator.RunSimulationAsync(runSimulationCommand, cancellationToken);
            var syncTask = dataSyncService.ExecuteAsync(portfolioId, closingDate.Date, cancellationToken);

            await Task.WhenAll(simulationTask, syncTask);

            if (simulationTask.Result.IsFailure)
                return Result.Failure<ClosedResult>(simulationTask.Result.Error);
            if (syncTask.Result.IsFailure)
                return Result.Failure<ClosedResult>(syncTask.Result.Error);

            // Paso 3: Valoración del Portafolio
            var valuationResult = await portfolioValuationService.CalculateAndPersistValuationAsync(portfolioId, closingDate, cancellationToken);
            if (valuationResult.IsFailure)
                return Result.Failure<ClosedResult>(valuationResult.Error);

            logger.LogInformation("Cierre finalizado exitosamente para portafolio {PortfolioId}", portfolioId);

            return Result.Success(new ClosedResult(portfolioId, closingDate));
        }
        catch (Exception ex)
        {
            await abortClosingService.AbortAsync(portfolioId, closingDate, cancellationToken);
            logger.LogError(ex, "Error inesperado en PrepareClosingOrchestrator para Portafolio {PortfolioId}", portfolioId);
            return Result.Failure<ClosedResult>(new Error("001","Error inesperado durante el cierre.", ErrorType.Failure));
        }
    }
    private async Task<Result<Unit>> ValidateBusinessRulesAsync(RunSimulationCommand parameters, CancellationToken ct)
    {
        var validation = await businessValidator.ValidateAsync(parameters, ct);
        return validation.IsFailure ? Result.Failure<Unit>(validation.Error!) : Result.Success(Unit.Value);
    }
}
