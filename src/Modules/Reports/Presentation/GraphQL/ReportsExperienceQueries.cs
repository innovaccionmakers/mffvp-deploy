using Common.SharedKernel.Application.Reports;
using Reports.Application.Strategies;

namespace Reports.Presentation.GraphQL
{
    public class ReportsExperienceQueries(
        IReportStrategyFactory strategyFactory) : IReportsExperienceQueries
    {
        public async Task<ReportResponseDto> GetReportDataAsync<TRequest>(TRequest request, ReportType reportType, CancellationToken cancellationToken = default)
        {
            var strategy = strategyFactory.GetStrategy(reportType);
            return await strategy.GetReportDataAsync(request, cancellationToken);
            throw new NotImplementedException();
        }
    }
}
