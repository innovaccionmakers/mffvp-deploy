using Common.SharedKernel.Domain;
using System.Text.Json;

namespace Reports.Domain.LoadingInfo.Audit;
public interface IEtlExecutionRepository
{
    Task<Result<long>> InsertRunningAsync(
        string executionName,
        JsonDocument? parametersJson,
        DateTimeOffset startedAtUtc,
        CancellationToken cancellationToken);

    Task<Result> FinalizeCompletedAsync(
        long executionId,
        DateTimeOffset finishedAtUtc,
        long durationMilliseconds,
        JsonDocument parametersFinalJson,
        CancellationToken cancellationToken);

    Task<Result> FinalizeFailedAsync(
        long executionId,
        DateTimeOffset finishedAtUtc,
        long durationMilliseconds,
        JsonDocument parametersFinalJson,
        JsonDocument errorJson,
        CancellationToken cancellationToken);

    /// <summary>
    /// Obtiene el timestamp de finalización (en milisegundos UNIX EPOCH) de la última ejecución exitosa
    /// para usar como filtro de RowVersion en las consultas incrementales.
    /// </summary>
    Task<long?> GetLastSuccessfulExecutionTimestampAsync(
        string executionName,
        CancellationToken cancellationToken);
}