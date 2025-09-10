using Common.SharedKernel.Application.Reports;
using Microsoft.AspNetCore.Mvc;

namespace Reports.Presentation.GraphQL
{
    public interface IReportsExperienceQueries
    {
        Task<IActionResult> GetReportDataAsync<TRequest>(TRequest request, ReportType reportType, CancellationToken cancellationToken = default);
    }
}
