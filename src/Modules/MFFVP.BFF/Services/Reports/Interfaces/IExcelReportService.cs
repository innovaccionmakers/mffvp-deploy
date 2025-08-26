using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services.Reports.Models;

namespace MFFVP.BFF.Services.Reports.Interfaces
{
    public interface IExcelReportService
    {
        Task<GraphqlResult<ReportResponseDto>> GetReportDataAsync<T>(T request, ReportType reportType, CancellationToken cancellationToken = default);
    }
}
