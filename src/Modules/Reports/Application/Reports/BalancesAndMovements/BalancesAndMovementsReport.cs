using Common.SharedKernel.Application.Reports;
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
                Rows = await GetSaldosData(reportRequest, cancellationToken)
            };
            worksheetDataList.Add(saldosData);

            // Hoja 2: Movimientos
            var movimientosData = new WorksheetData
            {
                WorksheetName = "Movimientos",
                ColumnHeaders = _movimientosHeaders,
                Rows = await GetMovimientosData(reportRequest, cancellationToken)
            };
            worksheetDataList.Add(movimientosData);

            return await Task.FromResult(worksheetDataList);
        }

        private async Task<List<object[]>> GetSaldosData(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            var dataList = new List<object[]>();

            foreach (var balanceResult in await reportRepository.GetBalancesAsync(reportRequest, cancellationToken))
            {
                if (balanceResult.IsSuccess && balanceResult.Value != null)
                {
                    var row = new object[]
                    {
                        balanceResult.Value.StartDate,
                        balanceResult.Value.StartDate,
                        balanceResult.Value.IdentificationType,
                        balanceResult.Value.Identification,
                        balanceResult.Value.FullName,
                        balanceResult.Value.TargetID,
                        balanceResult.Value.Target,
                        balanceResult.Value.Fund,
                        balanceResult.Value.Plan,
                        balanceResult.Value.Alternative,
                        balanceResult.Value.Portfolio,
                        balanceResult.Value.InitialBalance,
                        balanceResult.Value.Entry,
                        balanceResult.Value.Outflows,
                        balanceResult.Value.Returns,
                        balanceResult.Value.SourceWithholding,
                        balanceResult.Value.ClosingBalance
                    };

                    dataList.Add(row);
                }
            }

            return dataList;
        }

        private async Task<List<object[]>> GetMovimientosData(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            var dataList = new List<object[]>();

            foreach (var movementResult in await reportRepository.GetMovementsAsync(reportRequest, cancellationToken))
            {
                if (movementResult.IsSuccess && movementResult.Value != null)
                {
                    var row = new object[]
                    {
                        movementResult.Value.Date,
                        movementResult.Value.IdentificationType,
                        movementResult.Value.Identification,
                        movementResult.Value.AffiliateName,
                        movementResult.Value.TargetID,
                        movementResult.Value.Target,
                        movementResult.Value.FundName,
                        movementResult.Value.Plan,
                        movementResult.Value.Alternative,
                        movementResult.Value.Portfolio,
                        movementResult.Value.Receipt,
                        movementResult.Value.TransactionType,
                        movementResult.Value.TransactionSubtype,
                        movementResult.Value.Value,
                        movementResult.Value.TaxCondition,
                        movementResult.Value.ContingentWithholdingDue,
                        movementResult.Value.PaymentMethod
                    };

                    dataList.Add(row);
                }
            }

            return dataList;
        }
    }
}
