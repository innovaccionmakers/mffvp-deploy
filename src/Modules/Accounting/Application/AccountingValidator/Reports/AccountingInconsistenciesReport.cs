using Common.SharedKernel.Application.Reports.Strategies;
using Common.SharedKernel.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Accounting.Domain.AccountingInconsistencies;

namespace Accounting.Application.AccountingValidator.Reports;

public class AccountingInconsistenciesReport(ILogger<AccountingInconsistenciesReport> logger) : ExcelReportStrategyBase(logger)
{

    public override Task<IActionResult> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override string ReportName => "ValidacionesContabilidad";

    public override string[] ColumnHeaders => [
        "Portafolio",
        "Transacción",
        "Movimiento",
        "Inconsistencia"
    ];

    public async Task<FileStreamResult> GenerateFromDataAsync(DateTime processDate, IEnumerable<AccountingInconsistency> inconsistencies, CancellationToken cancellationToken)
    {
        var rows = inconsistencies.Select(inc => new object[]
        {
            inc.PortfolioId,
            inc.Transaction,
            inc.Activity ?? string.Empty,
            inc.Inconsistency
        }).ToList();

        var worksheetDataList = new List<WorksheetData>
        {
            new() {
                WorksheetName = WorksheetNames.AccountingInconsistencies,
                ColumnHeaders = ColumnHeaders,
                Rows = rows
            }
        };

        var fileName = GenerateReportFileName(processDate);
        return await GenerateExcelReportAsync(worksheetDataList, fileName, cancellationToken);
    }

    private string GenerateReportFileName(DateTime processDate)
    {
        var date = processDate.ToString("ddMMyyyy");
        return $"{ReportName}_{date}.xlsx";
    }
}
