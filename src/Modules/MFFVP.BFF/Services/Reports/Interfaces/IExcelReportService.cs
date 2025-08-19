using MFFVP.BFF.DTOs;

namespace MFFVP.BFF.Services.Reports.Interfaces
{
    public interface IExcelReportService
    {
        Task<ReportResponseDto> GetReportDataAsync<T>(T request, CancellationToken cancellationToken = default);
    }
}
