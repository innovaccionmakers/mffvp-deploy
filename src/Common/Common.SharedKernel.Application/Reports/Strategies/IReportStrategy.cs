using Microsoft.AspNetCore.Mvc;

namespace Common.SharedKernel.Application.Reports.Strategies;

public interface IReportStrategy
{
    Task<IActionResult> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken);
    string ReportName { get; }
    string[] ColumnHeaders { get; }
}
