using Common.SharedKernel.Application.Reports.Strategies;
using Common.SharedKernel.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Reports.Domain.Deposits;

namespace Reports.Application.Reports.Deposits
{
    public class DepositsReport(
        IDepositsRepository depositsRepository,
        ILogger<DepositsReport> logger) : ExcelReportStrategyBase(logger)
    {
        public override string ReportName => "Depositos";

        private readonly string[] _manualFormat = new[]
        {
            "Tipo de Cuenta", "Número de cuenta", "Código de la transacción",
            "Fecha efectiva (AAAAMMDD)", "Valor de la transacción", "Número de cheque",
            "Naturaleza C:Crédito D:Débito", "Observaciones", "Nombre de la transacción",
            "Detalle o información adicional", "Referencia # 1 de la transacción",
            "Referencia # 2 de la transacción", "Referencia # 3 de la transacción",
            "Sucursal de la transacción"
        };

        public override string[] ColumnHeaders => _manualFormat;

        public override async Task<IActionResult> GetReportDataAsync<TRequest>(
            TRequest request,
            CancellationToken cancellationToken)
        {
            DepositsRequest DepositsRequest = new DepositsRequest();

            if (request is DepositsRequest reportRequest)
            {
                DepositsRequest = reportRequest;
            }
            else if (request is DateTime processDate)
            {
                DepositsRequest = new DepositsRequest
                {
                    ProcessDate = processDate
                };
            }

            if (!DepositsRequest.IsValid())
            {
                logger.LogWarning("Request inválido recibido");
                throw new ArgumentException("El request no es válido");
            }

            try
            {
                return await GenerateExcelReportAsync(
                    ct => GetWorksheetDataAsync(DepositsRequest, ct),
                    $"{ReportName}{DepositsRequest.ProcessDate.ToString("ddMMyyyy")}.xlsx",
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error al generar el reporte con la fecha {DepositsRequest.ProcessDate}");
                throw;
            }
        }
        private async Task<List<WorksheetData>> GetWorksheetDataAsync(DepositsRequest reportRequest, CancellationToken cancellationToken)
        {
            var worksheetDataList = new List<WorksheetData>();

            var depositsData = new WorksheetData
            {
                WorksheetName = WorksheetNames.ManualFormat,
                ColumnHeaders = _manualFormat,
                Rows = await GetDepositsData(reportRequest, cancellationToken)
            };
            worksheetDataList.Add(depositsData);

            return await Task.FromResult(worksheetDataList);
        }

        private async Task<List<object[]>> GetDepositsData(DepositsRequest reportRequest, CancellationToken cancellationToken)
        {
            var depositsList = new List<object[]>();
            foreach (var deposits in await depositsRepository.GetDepositsAsync(reportRequest, cancellationToken))
            {
                var row = new object[]
                {
                    deposits.AccountType,
                    deposits.AccountNumber,
                    deposits.TransactionCode,
                    deposits.EffectiveDate.ToString("yyyyMMdd"),
                    deposits.TransactionValue,
                    deposits.CheckNumber,
                    deposits.Nature,
                    deposits.Observations,
                    deposits.TransactionName,
                    deposits.AdditionalInfo,
                    deposits.Reference1,
                    deposits.Reference2,
                    deposits.Reference3,
                    deposits.Branch
                };

                depositsList.Add(row);
            }

            return depositsList;
        }
    }
}
