using Common.SharedKernel.Application.Reports;
using Common.SharedKernel.Presentation.Results;

namespace MFFVP.BFF.Services.Reports.Strategies
{
    public interface IReportStrategy
    {
        Task<GraphqlResult<ReportResponseDto>> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken);
        string ReportName { get; }
        string[] ColumnHeaders { get; }
    }
}
