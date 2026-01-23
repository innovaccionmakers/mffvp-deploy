
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Reports.Domain.LoadingInfo.Audit.Dto;
using System.Diagnostics;

namespace Reports.Application.LoadingInfo.Audit;

public sealed class EtlAuditRunner : IEtlAuditRunner
{
    private readonly IEtlExecutionService executionService;

    public EtlAuditRunner(IEtlExecutionService executionService)
    {
        this.executionService = executionService;
    }

    public async Task<Result<EtlAuditRunResult>> RunAsync(
        string executionName,
        string correlationId,
        ExecutionParametersBuilder builder,
        IReadOnlyList<EtlWorkItem> workItems,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(executionName))
        {
            return Result.Failure<EtlAuditRunResult>(
                new Error("ETL001", "executionName es requerido.", ErrorType.Validation));
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            return Result.Failure<EtlAuditRunResult>(
                new Error("ETL002", "correlationId es requerido.", ErrorType.Validation));
        }

        if (builder is null)
        {
            return Result.Failure<EtlAuditRunResult>(
                new Error("ETL003", "builder es requerido.", ErrorType.Validation));
        }

        if (workItems is null || workItems.Count == 0)
        {
            return Result.Failure<EtlAuditRunResult>(
                new Error("ETL006", "Se requiere al menos un elemento de trabajo ETL.", ErrorType.Validation));
        }

        // Start: 1 insert
        var startResult = await executionService.StartAsync(executionName, builder, cancellationToken);

        if (!startResult.IsSuccess)
        {
            return Result.Failure<EtlAuditRunResult>(startResult.Error);
        }

        var auditExecutionId = startResult.Value;

        var overallStopwatch = Stopwatch.StartNew();

        try
        {
            var tasks = workItems
                .Select(item => RunSingleEtlAsync(builder, item, cancellationToken))
                .ToArray();

            var outcomes = await Task.WhenAll(tasks);

            overallStopwatch.Stop();

            var firstFailure = outcomes.FirstOrDefault(x => x.IsFailure);

            if (firstFailure is not null)
            {
                var problem = BuildProblemDetails(
                    executionName: executionName,
                    correlationId: correlationId,
                    auditExecutionId: auditExecutionId,
                    failedEtlName: firstFailure.EtlName,
                    errorCode: firstFailure.ErrorCode!,
                    errorMessage: firstFailure.ErrorMessage!,
                    traceId: Activity.Current?.Id);

                await executionService.FailAsync(
                    executionId: auditExecutionId,
                    builder: builder,
                    durationMilliseconds: overallStopwatch.ElapsedMilliseconds,
                    problemDetails: problem,
                    cancellationToken: cancellationToken);

                return Result.Success(new EtlAuditRunResult(
                    CorrelationId: correlationId,
                    AuditExecutionId: auditExecutionId,
                    IsSuccess: false,
                    FailedEtlName: firstFailure.EtlName,
                    ErrorCode: firstFailure.ErrorCode,
                    ErrorMessage: firstFailure.ErrorMessage
                ));
            }

            await executionService.CompleteAsync(
                executionId: auditExecutionId,
                builder: builder,
                durationMilliseconds: overallStopwatch.ElapsedMilliseconds,
                cancellationToken: cancellationToken);

            return Result.Success(new EtlAuditRunResult(
                CorrelationId: correlationId,
                AuditExecutionId: auditExecutionId,
                IsSuccess: true,
                FailedEtlName: null,
                ErrorCode: null,
                ErrorMessage: null
            ));
        }
        catch (OperationCanceledException)
        {
            overallStopwatch.Stop();

            var problem = BuildProblemDetails(
                executionName: executionName,
                correlationId: correlationId,
                auditExecutionId: auditExecutionId,
                failedEtlName: null,
                errorCode: "ETL499",
                errorMessage: "ETL ejecución cancelada.",
                traceId: Activity.Current?.Id);

            await executionService.FailAsync(
                executionId: auditExecutionId,
                builder: builder,
                durationMilliseconds: overallStopwatch.ElapsedMilliseconds,
                problemDetails: problem,
                cancellationToken: CancellationToken.None);

            return Result.Success(new EtlAuditRunResult(
                CorrelationId: correlationId,
                AuditExecutionId: auditExecutionId,
                IsSuccess: false,
                FailedEtlName: null,
                ErrorCode: "ETL499",
                ErrorMessage: "ETL ejecución cancelada."
            ));
        }
        catch (Exception exception)
        {
            overallStopwatch.Stop();

            var problem = BuildProblemDetails(
                executionName: executionName,
                correlationId: correlationId,
                auditExecutionId: auditExecutionId,
                failedEtlName: null,
                errorCode: "ETL500",
                errorMessage: exception.Message,
                traceId: Activity.Current?.Id);

            await executionService.FailAsync(
                executionId: auditExecutionId,
                builder: builder,
                durationMilliseconds: overallStopwatch.ElapsedMilliseconds,
                problemDetails: problem,
                cancellationToken: CancellationToken.None);

            return Result.Success(new EtlAuditRunResult(
                CorrelationId: correlationId,
                AuditExecutionId: auditExecutionId,
                IsSuccess: false,
                FailedEtlName: null,
                ErrorCode: "ETL500",
                ErrorMessage: exception.Message
            ));
        }
    }

    private static async Task<EtlWorkOutcome> RunSingleEtlAsync(
        ExecutionParametersBuilder builder,
        EtlWorkItem workItem,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        builder.MarkRunning(workItem.EtlName);

        try
        {
            var result = await workItem.RunAsync(cancellationToken);

            stopwatch.Stop();

            if (!result.IsSuccess)
            {
                builder.MarkFailed(workItem.EtlName, stopwatch.ElapsedMilliseconds);
                return EtlWorkOutcome.Fail(workItem.EtlName, result.Error.Code, result.Error.Description);
            }

            builder.MarkCompleted(
                workItem.EtlName,
                stopwatch.ElapsedMilliseconds,
                metrics: (IDictionary<string, long>?)result.Value.Metrics,
                warningCodes: result.Value.WarningCodes);

            return EtlWorkOutcome.Ok(workItem.EtlName);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();
            builder.MarkFailed(workItem.EtlName, stopwatch.ElapsedMilliseconds, warningCodes: new[] { "CANCELLED" });
            return EtlWorkOutcome.Fail(workItem.EtlName, "ETL499", "ETL paso cancelado.");
        }
        catch (Exception exception)
        {
            stopwatch.Stop();
            builder.MarkFailed(workItem.EtlName, stopwatch.ElapsedMilliseconds, warningCodes: new[] { "UNHANDLED" });
            return EtlWorkOutcome.Fail(workItem.EtlName, "ETL500", exception.Message);
        }
    }

    private static ExecutionProblemDetailsDto BuildProblemDetails(
        string executionName,
        string correlationId,
        long auditExecutionId,
        string? failedEtlName,
        string errorCode,
        string errorMessage,
        string? traceId)
    {
        var extensions = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["correlationId"] = correlationId,
            ["auditExecutionId"] = auditExecutionId.ToString()
        };

        if (!string.IsNullOrWhiteSpace(failedEtlName))
            extensions["failedEtlName"] = failedEtlName;

        return new ExecutionProblemDetailsDto(
            Type: "urn:makers:etl:execution:failed",
            Title: "ETL ejecución fallida",
            Status: 409,
            Code: errorCode,
            Detail: errorMessage,
            Instance: executionName,
            TraceId: traceId,
            Extensions: extensions
        );
    }

    private sealed record EtlWorkOutcome(string EtlName, bool IsFailure, string? ErrorCode, string? ErrorMessage)
    {
        public static EtlWorkOutcome Ok(string etlName) => new(etlName, false, null, null);
        public static EtlWorkOutcome Fail(string etlName, string code, string message) => new(etlName, true, code, message);
    }
}