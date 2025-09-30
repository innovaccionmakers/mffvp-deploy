using Accounting.Domain.AccountingInconsistencies;
using Common.SharedKernel.Core.Primitives;
namespace Accounting.Application.Abstractions;

    /// <summary>
    /// Interfaz para el manejo de inconsistencias en el procesamiento de comisiones contables
    /// </summary>
    public interface IInconsistencyHandler
    {
        /// <summary>
        /// Maneja las inconsistencias encontradas durante el procesamiento
        /// </summary>
        /// <param name="inconsistencies">Lista de inconsistencias encontrados</param>
        /// <param name="processDate">Fecha del proceso</param>
        /// <param name="processType">Tipo de proceso</param>
        /// <param name="cancellationToken">Token de cancelación</param>
        /// <returns>Task que representa la operación asíncrona</returns>
        Task HandleInconsistenciesAsync(
            IEnumerable<AccountingInconsistency> inconsistencies,
            DateTime processDate,
            string processType,
            CancellationToken cancellationToken = default);
    }
