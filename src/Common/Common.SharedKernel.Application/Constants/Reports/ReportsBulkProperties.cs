using Common.SharedKernel.Application.Constants;

namespace Common.SharedKernel.Application.Constants.Reports;

/// <summary>
/// Propiedades para operaciones bulk del módulo Reports.
/// </summary>
public static class ReportsBulkProperties
{
    /// <summary>
    /// Tamaño de lote para procesos ETL de carga de información.
    /// </summary>
    public const int EtlBatchSize = BulkOperationsConstants.DefaultBatchSize;
}