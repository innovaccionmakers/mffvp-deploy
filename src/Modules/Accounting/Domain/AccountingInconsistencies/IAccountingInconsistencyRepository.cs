using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;

namespace Accounting.Domain.AccountingInconsistencies;

/// <summary>
/// Repositorio para el manejo de inconsistencias contables en Redis
/// </summary>
public interface IAccountingInconsistencyRepository
{
    /// <summary>
    /// Guarda una lista de inconsistencias en Redis
    /// </summary>
    /// <param name="inconsistencies">Lista de inconsistencias a guardar</param>
    /// <param name="processDate">Fecha del proceso</param>
    /// <param name="processType">Tipo de proceso</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result> SaveInconsistenciesAsync(
        IEnumerable<AccountingInconsistency> inconsistencies,
        DateTime processDate,
        string processType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene las inconsistencias almacenadas para una fecha y tipo de proceso específicos
    /// </summary>
    /// <param name="processDate">Fecha del proceso</param>
    /// <param name="processType">Tipo de proceso</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Lista de inconsistencias encontradas</returns>
    Task<Result<IEnumerable<AccountingInconsistency>>> GetInconsistenciesAsync(
        DateTime processDate,
        string processType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina las inconsistencias almacenadas para una fecha y tipo de proceso específicos
    /// </summary>
    /// <param name="processDate">Fecha del proceso</param>
    /// <param name="processType">Tipo de proceso</param>
    /// <param name="cancellationToken">Token de cancelación</param>
    /// <returns>Resultado de la operación</returns>
    Task<Result> DeleteInconsistenciesAsync(
        DateTime processDate,
        string processType,
        CancellationToken cancellationToken = default);
}
