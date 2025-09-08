using Common.SharedKernel.Application.Reports;
using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services.Reports.Interfaces;
using MFFVP.BFF.Services.Reports.Strategies;

namespace MFFVP.BFF.Services.Reports
{
    public class ExcelReportService(
        IReportStrategyFactory strategyFactory) : IExcelReportService
    {
        public async Task<GraphqlResult<ReportResponseDto>> GetReportDataAsync<TRequest>(
            TRequest request,
            ReportType reportType,
            CancellationToken cancellationToken = default)
        {
            var strategy = strategyFactory.GetStrategy(reportType);
            return await strategy.GetReportDataAsync(request, cancellationToken);
        }
    }
}
