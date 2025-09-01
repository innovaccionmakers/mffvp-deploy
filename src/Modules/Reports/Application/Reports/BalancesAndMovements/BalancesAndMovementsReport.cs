using Common.SharedKernel.Application.Reports;
using DocumentFormat.OpenXml.Vml.Spreadsheet;
using Microsoft.Extensions.Logging;
using Reports.Application.Reports.Strategies;
using Reports.Domain.BalancesAndMovements;

namespace Reports.Application.Reports.BalancesAndMovements
{
    public class BalancesAndMovementsReport(
        ILogger<BalancesAndMovementsReport> logger,
        IBalancesAndMovementsReportRepository reportRepository) : ExcelReportStrategyBase(logger)
    {
        public override string ReportName => "Informe de Saldos y Movimientos";

        // Headers para la hoja de Saldos
        private readonly string[] _saldosHeaders = new[]
        {
            "Fecha Inicial", 
            "Fecha Final", 
            "Tipo Identificacion", 
            "Identificacion",
            "Nombre Afiliado", 
            "IdObjetivo", 
            "Objetivo", 
            "Nombre Fondo", 
            "Plan",
            "Alternativa", 
            "Portafolio", 
            "Saldo Inicial", 
            "Entradas", 
            "Salidas",
            "Rendimientos", 
            "Retefuente", "Saldo Final"
        };

        // Headers para la hoja de Movimientos
        private readonly string[] _movimientosHeaders = new[]
        {
            "Fecha",
            "Tipo Identificacion",
            "Identificacion",
            "Nombre Afiliado",
            "IdObjetivo",
            "Objetivo",
            "Nombre Fondo",
            "Plan",
            "Alternativa",
            "Portafolio",
            "Comprobante",
            "Tipo Transacción",
            "Subtipo Transacción",
            "Valor",
            "Condición Tributaria",
            "Retención Contingente Por Aplicar",
            "Forma de Pago",
        };

        public override string[] ColumnHeaders => _saldosHeaders; // Por defecto retorna los headers de saldos

        public override async Task<ReportResponseDto> GetReportDataAsync<TRequest>(
            TRequest request,
            CancellationToken cancellationToken)
        {
            BalancesAndMovementsReportRequest balancesAndMovementsReportRequest = new BalancesAndMovementsReportRequest();

            if (request is BalancesAndMovementsReportRequest reportRequest)
            {
                balancesAndMovementsReportRequest = reportRequest;
            }
            else if (request is ValueTuple<DateOnly, DateOnly, string> tuple)
            {
                balancesAndMovementsReportRequest = new BalancesAndMovementsReportRequest
                {
                    startDate = tuple.Item1,
                    endDate = tuple.Item2,
                    Identification = tuple.Item3
                };
            }

            if (!balancesAndMovementsReportRequest.IsValid())
            {
                logger.LogWarning("Request inválido recibido");
                throw new ArgumentException("El request no es válido");
            }

            try
            {
                return await GenerateExcelReportAsync(
                    ct => GetWorksheetDataAsync(balancesAndMovementsReportRequest, ct),
                    $"{ReportName}.xlsx",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error al generar el reporte con el rango de fechas de {balancesAndMovementsReportRequest.startDate} a {balancesAndMovementsReportRequest.endDate}",
                    balancesAndMovementsReportRequest.startDate.ToString("yyyy-MM-dd"), balancesAndMovementsReportRequest.endDate.ToString("yyyy-MM-dd"));
                throw;
                }
            
        }

        private async Task<List<WorksheetData>> GetWorksheetDataAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            var worksheetDataList = new List<WorksheetData>();

            // Hoja 1: Saldos
            var saldosData = new WorksheetData
            {
                WorksheetName = "Saldos",
                ColumnHeaders = _saldosHeaders,
                Rows = GetMockSaldosData(reportRequest, cancellationToken)
            };
            worksheetDataList.Add(saldosData);

            // Hoja 2: Movimientos
            var movimientosData = new WorksheetData
            {
                WorksheetName = "Movimientos",
                ColumnHeaders = _movimientosHeaders,
                Rows = GetMockMovimientosData(reportRequest, cancellationToken)
            };
            worksheetDataList.Add(movimientosData);

            return await Task.FromResult(worksheetDataList);
        }

        private List<object[]> GetMockSaldosData(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            var response = reportRepository.GetBalancesAsync(reportRequest, cancellationToken);
            // Datos de ejemplo basados en el CSV proporcionado
            return new List<object[]>
            {
            };
        }

        private List<object[]> GetMockMovimientosData(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            var response = reportRepository.GetMovementsAsync(reportRequest, cancellationToken);
            // Datos de ejemplo para movimientos
            return new List<object[]>
            {
                new object[]
                {
                    "14/07/2025", // Fecha
                    "CC - Cedula Ciudadanía", // Tipo Identificacion
                    "1152196365", // Identificacion
                    "Veronica Gutirrez Posada", // Nombre Afiliado
                    "123", // IdObjetivo
                    "1 - Inversiones", // Objetivo
                    "1 - FVP Cibest Capital", // Nombre Fondo
                    "1 - Abierto", // Plan
                    "1 - Multinversion - Autogestionada", // Alternativa
                    "1- Fiducuenta Pensión", // Portafolio
                    "326", // Comprobante
                    "A - Aporte", // Tipo Transacción
                    "N - Ninguno", // Subtipo Transacción
                    5000000m, // Valor
                    "Exento", // Condición Tributaria
                    0m, // Retención Contingente Por Aplicar
                    "T - Transferencia Bancaria" // Forma de Pago
                },
                new object[]
                {
                    "14/07/2025", // Fecha
                    "CC - Cedula Ciudadanía", // Tipo Identificacion
                    "1152196000", // Identificacion
                    "Juan Camilo Gomez", // Nombre Afiliado
                    "896", // IdObjetivo
                    "1 - Inversiones", // Objetivo
                    "1 - FVP Cibest Capital", // Nombre Fondo
                    "1 - Abierto", // Plan
                    "1 - Multinversion - Autogestionada", // Alternativa
                    "1- Fiducuenta Pensión", // Portafolio
                    "327", // Comprobante
                    "A - Aporte", // Tipo Transacción
                    "N - Ninguno", // Subtipo Transacción
                    2350000m, // Valor
                    "Sin Retención Contingente", // Condición Tributaria
                    0m, // Retención Contingente Por Aplicar
                    "T - Transferencia Bancaria" // Forma de Pago
                },
                new object[]
                {
                    "14/07/2025", // Fecha
                    "CC - Cedula Ciudadanía", // Tipo Identificacion
                    "1152196365", // Identificacion
                    "Fabian Arteaga", // Nombre Afiliado
                    "258", // IdObjetivo
                    "1 - Inversiones", // Objetivo
                    "1 - FVP Cibest Capital", // Nombre Fondo
                    "1 - Abierto", // Plan
                    "1 - Multinversion - Autogestionada", // Alternativa
                    "1- Fiducuenta Pensión", // Portafolio
                    "328", // Comprobante
                    "A - Aporte", // Tipo Transacción
                    "N - Ninguno", // Subtipo Transacción
                    10000000m, // Valor
                    "Con Retención Contigente Afiliado", // Condición Tributaria
                    700000m, // Retención Contingente Por Aplicar
                    "T - Transferencia Bancaria" // Forma de Pago
                },
                new object[]
                {
                    "15/07/2025", // Fecha
                    "CC - Cedula Ciudadanía", // Tipo Identificacion
                    "1152196365", // Identificacion
                    "Fabian Arteaga", // Nombre Afiliado
                    "258", // IdObjetivo
                    "1 - Inversiones", // Objetivo
                    "1 - FVP Cibest Capital", // Nombre Fondo
                    "1 - Abierto", // Plan
                    "1 - Multinversion - Autogestionada", // Alternativa
                    "1- Fiducuenta Pensión", // Portafolio
                    "329", // Comprobante
                    "A - Aporte", // Tipo Transacción
                    "N - Ninguno", // Subtipo Transacción
                    10000000m, // Valor
                    "Con Retención Contigente Afiliado", // Condición Tributaria
                    700000m, // Retención Contingente Por Aplicar
                    "T - Transferencia Bancaria" // Forma de Pago
                },
                // Puedes agregar más datos de ejemplo aquí si es necesario
                new object[]
                {
                    "16/07/2025", // Fecha
                    "CC - Cedula Ciudadanía", // Tipo Identificacion
                    "1152196365", // Identificacion
                    "Veronica Gutirrez Posada", // Nombre Afiliado
                    "123", // IdObjetivo
                    "1 - Inversiones", // Objetivo
                    "1 - FVP Cibest Capital", // Nombre Fondo
                    "1 - Abierto", // Plan
                    "1 - Multinversion - Autogestionada", // Alternativa
                    "1- Fiducuenta Pensión", // Portafolio
                    "330", // Comprobante
                    "R - Retiro", // Tipo Transacción
                    "P - Parcial", // Subtipo Transacción
                    1500000m, // Valor
                    "Exento", // Condición Tributaria
                    0m, // Retención Contingente Por Aplicar
                    "T - Transferencia Bancaria" // Forma de Pago
                },
                new object[]
                {
                    "17/07/2025", // Fecha
                    "CC - Cedula Ciudadanía", // Tipo Identificacion
                    "1152196000", // Identificacion
                    "Juan Camilo Gomez", // Nombre Afiliado
                    "896", // IdObjetivo
                    "1 - Inversiones", // Objetivo
                    "1 - FVP Cibest Capital", // Nombre Fondo
                    "1 - Abierto", // Plan
                    "1 - Multinversion - Autogestionada", // Alternativa
                    "1- Fiducuenta Pensión", // Portafolio
                    "331", // Comprobante
                    "R - Retiro", // Tipo Transacción
                    "T - Total", // Subtipo Transacción
                    500000m, // Valor
                    "Sin Retención Contingente", // Condición Tributaria
                    0m, // Retención Contingente Por Aplicar
                    "T - Transferencia Bancaria" // Forma de Pago
                }
            };
        }
    }
}
