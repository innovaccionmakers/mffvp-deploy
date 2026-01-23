using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;
using Reports.Application.LoadingInfo.Contracts;
using Reports.Application.LoadingInfo.Audit;
using Reports.Application.LoadingInfo.Services.Orchestrator;
using Reports.Domain.LoadingInfo.Audit.Dto;
using Reports.Domain.LoadingInfo.Constants;
using Reports.Integrations.LoadingInfo.commands;
using Reports.Integrations.LoadingInfo.Commands;
using System.Collections.Concurrent;

namespace Reports.Application.LoadingInfo.Orchestration;

public sealed class LoadingInfoOrchestrator : ILoadingInfoOrchestrator
{
    private readonly IPeopleLoader _peopleLoader;
    private readonly IProductsLoader _productsLoader;
    private readonly IClosingLoader _closingLoader;
    private readonly IBalancesLoader _balancesLoader;
    private readonly IEtlAuditRunner _etlAuditRunner;
    private readonly ILogger<LoadingInfoOrchestrator> _logger;

    public LoadingInfoOrchestrator(
        IPeopleLoader peopleLoader,
        IProductsLoader productsLoader,
        IClosingLoader closingLoader,
        IBalancesLoader balancesLoader,
        IEtlAuditRunner etlAuditRunner,
        ILogger<LoadingInfoOrchestrator> logger)
    {
        _peopleLoader = peopleLoader;
        _productsLoader = productsLoader;
        _closingLoader = closingLoader;
        _balancesLoader = balancesLoader;
        _etlAuditRunner = etlAuditRunner;
        _logger = logger;
    }

    public async Task<Result<LoadingInfoRunResponse>> RunAsync(
        string executionId,
        EtlSelection etlSelection,
        DateTime closingDateUtc,
        int portfolioId,
        CancellationToken cancellationToken)
    {
        if (etlSelection == EtlSelection.None)
        {
            return Result.Failure<LoadingInfoRunResponse>(
            new Error("ETLSEL_001", "Se debe seleccionar al menos un ETL.", ErrorType.Validation));
        }

        if (portfolioId <= 0)
        {
            return Result.Failure<LoadingInfoRunResponse>(
            new Error("ETLSEL_002", "PortfolioId debe ser un número entero positivo.", ErrorType.Validation));
        }

        var steps = BuildSteps(etlSelection, closingDateUtc, portfolioId);

        _logger.LogInformation(
            "LoadingInfo inició. CorrelationId={CorrelationId} Selection={Selection} Steps={StepCount} ClosingDateUtc={ClosingDateUtc} PortfolioId={PortfolioId}",
            executionId, etlSelection, steps.Count, closingDateUtc, portfolioId);

        var builder = new ExecutionParametersBuilder(
         etlSelection: etlSelection.ToString(),
         etlNames: steps.Select(x => x.EtlName))
        .WithCorrelationId(executionId)
        .WithPortfolioId(portfolioId)
         .WithClosingDateUtc(closingDateUtc);

        var responses = new ConcurrentBag<EtlStepResponse>();

        var workItems = steps.Select(step => new EtlWorkItem(
             EtlName: step.EtlName,
             RunAsync: async ct =>
            {
                var stepResponse = await ExecuteStepAsync(step, executionId, ct);
                responses.Add(stepResponse);

                if (!stepResponse.IsSuccess)
                {
                    return Result.Failure<EtlWorkResult>(
                     new Error(stepResponse.ErrorCode ?? "ETL_STEP_FAILED",
                           stepResponse.ErrorMessage ?? "ETL step falló.",
                           ErrorType.Failure));
                }

                return Result.Success(new EtlWorkResult(
                        Metrics: new Dictionary<string, long>
                        {
                            ["readRows"] = stepResponse.ReadRows,
                            ["insertedRows"] = stepResponse.InsertedRows
                        },
                        WarningCodes: null));
            }))
            .ToList();

        var auditResult = await _etlAuditRunner.RunAsync(
            executionName: "loading-info",
            correlationId: executionId,
            builder: builder,
            workItems: workItems,
            cancellationToken: cancellationToken);

        if (!auditResult.IsSuccess)
        {
            return Result.Failure<LoadingInfoRunResponse>(auditResult.Error);
        }

        var orderedSteps = responses
            .OrderBy(r => r.EtlName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var outcome = auditResult.Value;

        if (!outcome.IsSuccess)
        {
            _logger.LogWarning(
             "LoadingInfo finalizó con errores. CorrelationId={CorrelationId} AuditExecutionId={AuditExecutionId} FailedEtl={FailedEtl} ErrorCode={ErrorCode}",
            outcome.CorrelationId, outcome.AuditExecutionId, outcome.FailedEtlName, outcome.ErrorCode);

            var errorMessage =
          "Uno o más pasos de ETL fallaron: " +
                  string.Join(" | ", orderedSteps
                  .Where(x => !x.IsSuccess)
                  .Select(f => $"{f.EtlName} [{f.ErrorCode}]: {f.ErrorMessage}"));

            return Result.Success(new LoadingInfoRunResponse(
             CorrelationId: outcome.CorrelationId,
             AuditExecutionId: outcome.AuditExecutionId,
             IsSuccess: false,
             Steps: orderedSteps,
             ErrorCode: "ETL_ORCH_MULTI_ERROR",
             ErrorMessage: errorMessage
            ));
        }

        _logger.LogInformation(
         "LoadingInfo finalizó exitosamente. CorrelationId={CorrelationId} AuditExecutionId={AuditExecutionId}",
         outcome.CorrelationId, outcome.AuditExecutionId);

        return Result.Success(new LoadingInfoRunResponse(
            CorrelationId: outcome.CorrelationId,
            AuditExecutionId: outcome.AuditExecutionId,
            IsSuccess: true,
            Steps: orderedSteps,
            ErrorCode: null,
            ErrorMessage: null
         ));
    }

    private List<EtlStepDefinition> BuildSteps(EtlSelection selection, DateTime closingDateUtc, int portfolioId)
    {
        var steps = new List<EtlStepDefinition>();

        if (selection.HasFlag(EtlSelection.AffiliatesClients))
        {
            steps.Add(new EtlStepDefinition(
                    EtlName: "ETLafiliado_clientes",
                     ExecuteAsync: async ct =>
                      {
                          var result = await _peopleLoader.ExecuteAsync(ct);
                          return result.IsSuccess
                         ? (result.Value.ReadRows, result.Value.InsertedRows, null)
                         : (0L, 0L, result.Error);
                      }));
        }

        if (selection.HasFlag(EtlSelection.Products))
        {
            steps.Add(new EtlStepDefinition(
                EtlName: "ETLProductos",
                ExecuteAsync: async ct =>
             {
                 var result = await _productsLoader.ExecuteAsync(ct);
                 return result.IsSuccess
              ? (result.Value.ReadRows, result.Value.InsertedRows, null)
              : (0L, 0L, result.Error);
             }));
        }

        if (selection.HasFlag(EtlSelection.Closing))
        {
            steps.Add(new EtlStepDefinition(
                 EtlName: "ETLCierre",
                 ExecuteAsync: async ct =>
                {
                    var result = await _closingLoader.ExecuteAsync(closingDateUtc, portfolioId, ct);
                    return result.IsSuccess
                 ? (result.Value.ReadRows, result.Value.InsertedRows, null)
                        : (0L, 0L, result.Error);
                }));
        }

        if (selection.HasFlag(EtlSelection.Balances))
        {
            steps.Add(new EtlStepDefinition(
                EtlName: "ETLSaldos",
                ExecuteAsync: async ct =>
                 {
                     var result = await _balancesLoader.ExecuteAsync(closingDateUtc, portfolioId, ct);
                     return result.IsSuccess
                              ? (result.Value.ReadRows, result.Value.InsertedRows, null)
                            : (0L, 0L, result.Error);
                 }));
        }
        return steps;
    }

    private async Task<EtlStepResponse> ExecuteStepAsync(
        EtlStepDefinition step,
        string executionId,
        CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            _logger.LogInformation(
              "ETL Step inició. CorrelationId={CorrelationId} EtlName={EtlName}",
           executionId, step.EtlName);

            var (readRows, insertedRows, error) = await step.ExecuteAsync(cancellationToken);

            stopwatch.Stop();

            if (error is not null)
            {
                _logger.LogWarning(
                  "ETL Step falló. CorrelationId={CorrelationId} EtlName={EtlName} DurationMs={DurationMs} ErrorCode={ErrorCode} ErrorMessage={ErrorMessage}",
                    executionId, step.EtlName, stopwatch.ElapsedMilliseconds, error.Code, error.Description);

                return new EtlStepResponse(
                    EtlName: step.EtlName,
                    ReadRows: 0,
                    InsertedRows: 0,
                    IsSuccess: false,
                    ErrorCode: error.Code,
                    ErrorMessage: error.Description);
            }

            _logger.LogInformation(
                "ETL Step finalizó. CorrelationId={CorrelationId} EtlName={EtlName} DurationMs={DurationMs} ReadRows={ReadRows} InsertedRows={InsertedRows}",
                executionId, step.EtlName, stopwatch.ElapsedMilliseconds, readRows, insertedRows);

            return new EtlStepResponse(
                    EtlName: step.EtlName,
                    ReadRows: readRows,
                    InsertedRows: insertedRows,
                    IsSuccess: true,
                    ErrorCode: null,
                    ErrorMessage: null);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            throw;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            _logger.LogError(
                    exception,
                     "ETL Step error inesperado. CorrelationId={CorrelationId} EtlName={EtlName} DurationMs={DurationMs}",
                    executionId, step.EtlName, stopwatch.ElapsedMilliseconds);

            return new EtlStepResponse(
                       EtlName: step.EtlName,
                        ReadRows: 0,
                        InsertedRows: 0,
                        IsSuccess: false,
                        ErrorCode: "ETL_ORCH_UNEXPECTED",
                        ErrorMessage: exception.Message);
        }
    }
}