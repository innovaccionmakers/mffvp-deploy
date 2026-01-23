using Common.SharedKernel.Domain;
using Common.SharedKernel.Core.Primitives;
using System.Diagnostics;
using Reports.Domain.LoadingInfo.Audit;
using Reports.Domain.LoadingInfo.Audit.Dto;

namespace Reports.Application.LoadingInfo.Audit;

public sealed class EtlExecutionService : IEtlExecutionService
{
    private readonly IEtlExecutionRepository repository;

    public EtlExecutionService(IEtlExecutionRepository repository)
    {
        this.repository = repository;
    }

    public Task<Result<long>> StartAsync(
        string executionName,
        ExecutionParametersBuilder builder,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(executionName))
            return Task.FromResult(Result.Failure<long>(
                    new Error("ETL001", "executionName es requerido.", ErrorType.Validation)));

        if (builder is null)
            return Task.FromResult(Result.Failure<long>(
                new Error("ETL003", "builder es requerido.", ErrorType.Validation)));

        var startedAtUtc = DateTimeOffset.UtcNow;
        var parametersJson = builder.BuildFinalJsonDocument();

        return repository.InsertRunningAsync(
            executionName: executionName,
            parametersJson: parametersJson,
            startedAtUtc: startedAtUtc,
            cancellationToken: cancellationToken);
    }

    public Task<Result> CompleteAsync(
        long executionId,
        ExecutionParametersBuilder builder,
        long durationMilliseconds,
        CancellationToken cancellationToken)
    {
        if (executionId <= 0)
            return Task.FromResult(Result.Failure(
                new Error("ETL002", "executionId debe ser un número positivo.", ErrorType.Validation)));

        if (builder is null)
            return Task.FromResult(Result.Failure(
                new Error("ETL003", "builder es requerido.", ErrorType.Validation)));

        var finishedAtUtc = DateTimeOffset.UtcNow;
        var parametersFinalJson = builder.BuildFinalJsonDocument();

        return repository.FinalizeCompletedAsync(
            executionId: executionId,
            finishedAtUtc: finishedAtUtc,
            durationMilliseconds: durationMilliseconds,
            parametersFinalJson: parametersFinalJson,
            cancellationToken: cancellationToken);
    }

    public Task<Result> FailAsync(
        long executionId,
        ExecutionParametersBuilder builder,
        long durationMilliseconds,
        ExecutionProblemDetailsDto problemDetails,
        CancellationToken cancellationToken)
    {
        if (executionId <= 0)
            return Task.FromResult(Result.Failure(
                new Error("ETL002", "executionId debe ser un número positivo.", ErrorType.Validation)));

        if (builder is null)
            return Task.FromResult(Result.Failure(
                new Error("ETL003", "builder es requerido.", ErrorType.Validation)));

        if (problemDetails is null)
            return Task.FromResult(Result.Failure(
                new Error("ETL005", "problemDetails es requerido.", ErrorType.Validation)));

        var finishedAtUtc = DateTimeOffset.UtcNow;
        var parametersFinalJson = builder.BuildFinalJsonDocument();

        var traceId = problemDetails.TraceId ?? Activity.Current?.Id;
        var normalizedProblem = traceId == problemDetails.TraceId
            ? problemDetails
            : problemDetails with { TraceId = traceId };

        var errorJson = EtlJson.ToJsonDocument(normalizedProblem);

        return repository.FinalizeFailedAsync(
            executionId: executionId,
            finishedAtUtc: finishedAtUtc,
            durationMilliseconds: durationMilliseconds,
            parametersFinalJson: parametersFinalJson,
            errorJson: errorJson,
            cancellationToken: cancellationToken);
    }
}