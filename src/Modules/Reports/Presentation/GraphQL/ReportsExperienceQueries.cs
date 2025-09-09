using Common.SharedKernel.Application.Reports;
using Microsoft.AspNetCore.Mvc;
using Reports.Application.Reports.Strategies;

namespace Reports.Presentation.GraphQL
{
    public class ReportsExperienceQueries(
        IReportStrategyFactory strategyFactory) : IReportsExperienceQueries
    {
        public async Task<IActionResult> GetReportDataAsync<TRequest>(TRequest request, ReportType reportType, CancellationToken cancellationToken = default)
        {
            var strategy = strategyFactory.GetStrategy(reportType);
            return await strategy.GetReportDataAsync(request, cancellationToken);
        }
    }
}
