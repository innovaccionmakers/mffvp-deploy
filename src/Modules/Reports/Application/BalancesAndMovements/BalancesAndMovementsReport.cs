using Common.SharedKernel.Application.Reports;
using Microsoft.Extensions.Logging;
using Reports.Application.Strategies;
using Reports.Domain.BalancesAndMovements;

namespace Reports.Application.BalancesAndMovements
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
            if (request is BalancesAndMovementsReportRequest reportRequest)
            {
                if (!reportRequest.IsValid())
                {
                    logger.LogWarning("Request inválido recibido");
                    throw new ArgumentException("El request no es válido");
                }

                try
                {
                    return await GenerateExcelReportAsync(
                        ct => GetWorksheetDataAsync(reportRequest, ct),
                        $"{ReportName}.xlsx",
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Error al generar el reporte con el rango de fechas de {reportRequest.startDate} a {reportRequest.endDate}",
                        reportRequest.startDate.ToString("yyyy-MM-dd"), reportRequest.endDate.ToString("yyyy-MM-dd"));
                    throw;
                }
            }
            else
            {
                logger.LogError("Tipo de request no válido. Se esperaba DateTime, se recibió: {RequestType}",
                    typeof(TRequest).Name);
                throw new ArgumentException("El tipo de request no es válido. Se esperaba DateTime.");
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
                Rows = GetMockSaldosData(reportRequest)
            };
            worksheetDataList.Add(saldosData);

            // Hoja 2: Movimientos
            var movimientosData = new WorksheetData
            {
                WorksheetName = "Movimientos",
                ColumnHeaders = _movimientosHeaders,
                Rows = GetMockMovimientosData(reportRequest)
            };
            worksheetDataList.Add(movimientosData);

            return await Task.FromResult(worksheetDataList);
        }

        private List<object[]> GetMockSaldosData(BalancesAndMovementsReportRequest reportRequest)
        {
            var response = reportRepository.GetBalancesAsync(reportRequest);
            // Datos de ejemplo basados en el CSV proporcionado
            return new List<object[]>
            {
                new object[]
                {
                    "14/07/2025",
                    "14/07/2025",
                    "CC - Cedula Ciudadanía",
                    "1152196365",
                    "Veronica Gutirrez Posada",
                    "123",
                    "1 - Inversiones",
                    "1 - FVP Cibest Capital",
                    "1 - Abierto",
                    "1 - Multinversion - Autogestionada",
                    "1- Fiducuenta Pensión",
                    2000000m,
                    100000m,
                    0m,
                    1280m,
                    0m,
                    2101280m
                },
                new object[]
                {
                    "14/07/2025",
                    "14/07/2025",
                    "CC - Cedula Ciudadanía",
                    "1152196365",
                    "Veronica Gutirrez Posada",
                    "124",
                    "1 - Inversiones",
                    "1 - FVP Cibest Capital",
                    "1 - Abierto",
                    "1 - Multinversion - Autogestionada",
                    "1- Fiducuenta Pensión",
                    0m,
                    650000m,
                    0m,
                    8320m,
                    0m,
                    658320m
                },
                new object[]
                {
                    "14/07/2025",
                    "14/07/2025",
                    "CC - Cedula Ciudadanía",
                    "1152196000",
                    "Juan Camilo Gomez",
                    "896",
                    "1 - Inversiones",
                    "1 - FVP Cibest Capital",
                    "1 - Abierto",
                    "1 - Multinversion - Autogestionada",
                    "1- Fiducuenta Pensión",
                    0m,
                    0m,
                    0m,
                    0m,
                    0m,
                    0m
                },
                new object[]
                {
                    "14/07/2025",
                    "14/07/2025",
                    "CC - Cedula Ciudadanía",
                    "1152196365",
                    "Fabian Arteaga",
                    "258",
                    "1 - Inversiones",
                    "1 - FVP Cibest Capital",
                    "1 - Abierto",
                    "1 - Multinversion - Autogestionada",
                    "1- Fiducuenta Pensión",
                    0m,
                    2200000m,
                    0m,
                    28160m,
                    0m,
                    2228160m
                }
            };
        }

        private List<object[]> GetMockMovimientosData(BalancesAndMovementsReportRequest reportRequest)
        {
            var response = reportRepository.GetMovementsAsync(reportRequest);
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
