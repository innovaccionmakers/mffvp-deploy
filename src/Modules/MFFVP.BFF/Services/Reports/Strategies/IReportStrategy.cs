using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services.Reports.Models;

namespace MFFVP.BFF.Services.Reports.Strategies
{
    public interface IReportStrategy
    {
        Task<GraphqlResult<ReportResponseDto>> GetReportDataAsync<TRequest>(TRequest processDate, CancellationToken cancellationToken);
        string ReportName { get; }
        string[] ColumnHeaders { get; }
    }
}
