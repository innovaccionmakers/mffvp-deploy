using Reports.Application.DTOs;
using Reports.Infrastructure.Models;

namespace Reports.Infrastructure.Interfaces
{
    public interface IExcelReportService
    {
        Task<ReportResponseDto> GetReportDataAsync<T>(T request, ReportType reportType, CancellationToken cancellationToken = default);
    }
}
