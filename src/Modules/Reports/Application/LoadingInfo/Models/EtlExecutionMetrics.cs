namespace Reports.Application.LoadingInfo.Models;

/// <summary>
/// Métricas de ejecución de un proceso ETL.
/// </summary>
/// <param name="ReadRows">Número de filas leídas desde la fuente.</param>
/// <param name="InsertedRows">Número de filas insertadas en destino.</param>
public sealed record EtlExecutionMetrics(long ReadRows, long InsertedRows);
