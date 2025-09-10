using Common.SharedKernel.Application.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Reports.Application.Reports.Strategies
{
    public interface IReportStrategy
    {
        Task<IActionResult> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken);
        string ReportName { get; }
        string[] ColumnHeaders { get; }
    }
}
