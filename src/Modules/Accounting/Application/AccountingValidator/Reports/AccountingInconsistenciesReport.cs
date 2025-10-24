using Common.SharedKernel.Application.Reports.Strategies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.AccountingValidator.Reports;

public class AccountingInconsistenciesReport(ILogger<AccountingInconsistenciesReport> logger) : ExcelReportStrategyBase(logger)
{
    public override string ReportName => "ValidacionesContabilidad";

    public override string[] ColumnHeaders => [
        "Portafolio",
        "Transacción",
        "Movimiento",
        "Inconsistencia"
    ];

    public override async Task<IActionResult> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        if (request is AccountingInconsistenciesRequest reportRequest)
        {
            return await GenerateExcelReportAsync(
                ct => GetWorksheetDataAsync(reportRequest, ct),
                GenerateReportFileName(reportRequest),
                cancellationToken);
        }
        else
        {
            logger.LogError("Tipo de request no válido. Se esperaba AccountingInconsistenciesRequest, se recibió: {RequestType}",
                request?.GetType().FullName ?? "null");
            throw new ArgumentException("Tipo de request no válido");
        }
    }



    private Task<List<WorksheetData>> GetWorksheetDataAsync(AccountingInconsistenciesRequest reportRequest, CancellationToken cancellationToken)
    {
        var worksheetDataList = new List<WorksheetData>();

        var rows = reportRequest.Inconsistencies.Select(inc => new object[]
        {
            inc.PortfolioId,
            inc.Transaction,
            inc.Activity ?? "",
            inc.Inconsistency
        }).ToList();

        var result = new WorksheetData
        {
            WorksheetName = "InconsistenciasContables",
            ColumnHeaders = ColumnHeaders,
            Rows = rows ?? []
        };
        worksheetDataList.Add(result);

        return Task.FromResult(worksheetDataList);

    }

    private string GenerateReportFileName(AccountingInconsistenciesRequest reportRequest)
    {
        var processDate = reportRequest.ProcessDate.ToString("ddMMyyyy");
        return $"{ReportName}_{processDate}.xlsx";
    }
}
