using Common.SharedKernel.Domain;
using Reports.Application.LoadingInfo.Models;

namespace Reports.Application.LoadingInfo.Contracts;

/// <summary>
/// Interfaz base para loaders ETL sin parámetros.
/// </summary>
public interface IEtlLoader
{
    /// <summary>
    /// Ejecuta el proceso ETL.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Métricas de ejecución del ETL.</returns>
    Task<Result<EtlExecutionMetrics>> ExecuteAsync(CancellationToken cancellationToken);
}
