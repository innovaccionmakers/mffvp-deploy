using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services.Reports.Models;

namespace MFFVP.BFF.Services.Reports.Strategies
{
    public interface IReportStrategy
    {
        Task<ReportResponseDto> GetReportDataAsync<TRequest>(TRequest processDate, CancellationToken cancellationToken);
        string ReportName { get; }
        string[] ColumnHeaders { get; }
    }
}
