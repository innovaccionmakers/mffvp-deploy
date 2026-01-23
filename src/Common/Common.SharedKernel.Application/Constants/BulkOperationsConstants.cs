namespace Common.SharedKernel.Application.Constants;

/// <summary>
/// Constantes compartidas para operaciones masivas (bulk operations) en toda la aplicación.
/// </summary>
public static class BulkOperationsConstants
{
    /// <summary>
    /// Tamaño de lote estándar para operaciones de inserción/actualización masiva.
    /// Valor optimizado para PostgreSQL y SQL Server.
    /// </summary>
    public const int DefaultBatchSize = 10_000;
    
    /// <summary>
    /// Tamaño de lote pequeño para operaciones con registros grandes o complejos.
    /// </summary>
    public const int SmallBatchSize = 1_000;
    
    /// <summary>
    /// Tamaño de lote grande para operaciones con registros simples.
    /// </summary>
    public const int LargeBatchSize = 50_000;
}