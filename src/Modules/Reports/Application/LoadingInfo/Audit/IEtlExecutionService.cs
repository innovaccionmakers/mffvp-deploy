using Common.SharedKernel.Domain;
using Reports.Domain.LoadingInfo.Audit.Dto;

namespace Reports.Application.LoadingInfo.Audit;

public interface IEtlExecutionService
{
    Task<Result<long>> StartAsync(
        string executionName,
        ExecutionParametersBuilder builder,
        CancellationToken cancellationToken);

    Task<Result> CompleteAsync(
        long executionId,
        ExecutionParametersBuilder builder,
        long durationMilliseconds,
        CancellationToken cancellationToken);

    Task<Result> FailAsync(
        long executionId,
        ExecutionParametersBuilder builder,
        long durationMilliseconds,
        ExecutionProblemDetailsDto problemDetails,
        CancellationToken cancellationToken);
}