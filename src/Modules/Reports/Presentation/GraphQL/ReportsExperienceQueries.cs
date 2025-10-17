using Common.SharedKernel.Application.Reports;
using Common.SharedKernel.Application.Reports.Strategies;
using Microsoft.AspNetCore.Mvc;

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
