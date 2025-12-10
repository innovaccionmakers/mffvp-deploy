using Common.SharedKernel.Application.Reports.Strategies;
using Common.SharedKernel.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Reports.Domain.BalancesAndMovements;

namespace Reports.Application.Reports.BalancesAndMovements
{
    public class BalancesAndMovementsReport(
        ILogger<BalancesAndMovementsReport> logger,
        IBalancesAndMovementsReportRepository reportRepository) : ExcelReportStrategyBase(logger)
    {
        public override string ReportName => "Informe de Saldos y Movimientos";

        // Headers para la hoja de Saldos
        private readonly string[] _balancesHeaders = new[]
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
            "Retefuente", 
            "Saldo Final"
        };

        // Headers para la hoja de Movimientos
        private readonly string[] _movementsHeaders = new[]
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
            "Comercial",
            "Hora",
            "Cuenta Origen"

        };

        public override string[] ColumnHeaders => _balancesHeaders; // Por defecto retorna los headers de saldos

        public override async Task<IActionResult> GetReportDataAsync<TRequest>(
            TRequest request,
            CancellationToken cancellationToken)
        {
            BalancesAndMovementsReportRequest balancesAndMovementsReportRequest = new BalancesAndMovementsReportRequest();

            if (request is BalancesAndMovementsReportRequest reportRequest)
            {
                balancesAndMovementsReportRequest = reportRequest;
            }
            else if (request is ValueTuple<DateTime, DateTime, string> tuple)
            {
                balancesAndMovementsReportRequest = new BalancesAndMovementsReportRequest
                {
                    StartDate = tuple.Item1,
                    EndDate = tuple.Item2,
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
                logger.LogError(ex, $"Error al generar el reporte con el rango de fechas de {balancesAndMovementsReportRequest.StartDate} a {balancesAndMovementsReportRequest.EndDate}",
                    balancesAndMovementsReportRequest.StartDate.ToString("yyyy-MM-dd"), balancesAndMovementsReportRequest.EndDate.ToString("yyyy-MM-dd"));
                throw;
                }
            
        }

        private async Task<List<WorksheetData>> GetWorksheetDataAsync(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            var worksheetDataList = new List<WorksheetData>();

            // Hoja 1: Saldos
            var balancesData = new WorksheetData
            {
                WorksheetName = WorksheetNames.Balances,
                ColumnHeaders = _balancesHeaders,
                Rows = await GetBalancesData(reportRequest, cancellationToken)
            };
            worksheetDataList.Add(balancesData);

            // Hoja 2: Movimientos
            var movementsData = new WorksheetData
            {
                WorksheetName = WorksheetNames.Movements,
                ColumnHeaders = _movementsHeaders,
                Rows = await GetMovementsData(reportRequest, cancellationToken)
            };
            worksheetDataList.Add(movementsData);

            return await Task.FromResult(worksheetDataList);
        }

        private async Task<List<object[]>> GetBalancesData(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            var dataList = new List<object[]>();

            foreach (var balanceResult in await reportRepository.GetBalancesAsync(reportRequest, cancellationToken))
            {
                var row = new object[]
                {
                    balanceResult.StartDate,
                    balanceResult.EndDate,
                    balanceResult.IdentificationType,
                    balanceResult.Identification,
                    balanceResult.FullName,
                    balanceResult.ObjectiveId,
                    balanceResult.Objective,
                    balanceResult.Fund,
                    balanceResult.Plan,
                    balanceResult.Alternative,
                    balanceResult.Portfolio,
                    balanceResult.InitialBalance,
                    balanceResult.Entry,
                    balanceResult.Outflows,
                    balanceResult.Returns,
                    balanceResult.SourceWithholding,
                    balanceResult.ClosingBalance
                };

                dataList.Add(row);                
            }

            return dataList;
        }

        private async Task<List<object[]>> GetMovementsData(BalancesAndMovementsReportRequest reportRequest, CancellationToken cancellationToken)
        {
            var dataList = new List<object[]>();

            foreach (var movementResult in await reportRepository.GetMovementsAsync(reportRequest, cancellationToken))
            {  
                var row = new object[]
                {
                    movementResult.ProcesDate,
                    movementResult.IdentificationType,
                    movementResult.Identification,
                    movementResult.FullName,
                    movementResult.ObjectiveId,
                    movementResult.Objective,
                    movementResult.Fund,
                    movementResult.Plan,
                    movementResult.Alternative,
                    movementResult.Portfolio,
                    movementResult.Voucher,
                    movementResult.TransactionType,
                    movementResult.TransactionSubtype,
                    movementResult.Value,
                    movementResult.TaxCondition,
                    movementResult.ContingentWithholding,
                    movementResult.PaymentMethod,
                    movementResult.CommercialUser,
                    movementResult.Hour, 
                    movementResult.OriginAccount
                };

                dataList.Add(row);                
            }

            return dataList;
        }
    }
}
