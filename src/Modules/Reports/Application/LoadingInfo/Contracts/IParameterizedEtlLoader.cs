using Common.SharedKernel.Domain;
using Reports.Application.LoadingInfo.Models;

namespace Reports.Application.LoadingInfo.Contracts;

/// <summary>
/// Interfaz para loaders ETL que requieren fecha de cierre y portafolio.
/// </summary>
public interface IParameterizedEtlLoader
{
    /// <summary>
    /// Ejecuta el proceso ETL con parámetros de fecha y portafolio.
    /// </summary>
    /// <param name="closingDateUtc">Fecha de cierre en UTC.</param>
    /// <param name="portfolioId">ID del portafolio.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Métricas de ejecución del ETL.</returns>
    Task<Result<EtlExecutionMetrics>> ExecuteAsync(
        DateTime closingDateUtc,
        int portfolioId,
        CancellationToken cancellationToken);
}
