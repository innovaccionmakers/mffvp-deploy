using Azure.Core;
using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services.Reports.Interfaces;
using MFFVP.BFF.Services.Reports.Strategies;

namespace MFFVP.BFF.Services.Reports
{
    public class ExcelReportService(
        IReportStrategy strategy) : IExcelReportService
    {
        public async Task<GraphqlResult<ReportResponseDto>> GetReportDataAsync<TRequest>(
            TRequest request,
            CancellationToken cancellationToken = default)
        {
            return await strategy.GetReportDataAsync(request, cancellationToken);
        }
    }
}
