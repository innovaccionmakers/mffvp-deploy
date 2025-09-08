using Common.SharedKernel.Application.Reports;
using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.DTOs;

namespace MFFVP.BFF.Services.Reports.Interfaces
{
    public interface IExcelReportService
    {
        Task<GraphqlResult<ReportResponseDto>> GetReportDataAsync<T>(T request, ReportType reportType, CancellationToken cancellationToken = default);
    }
}
