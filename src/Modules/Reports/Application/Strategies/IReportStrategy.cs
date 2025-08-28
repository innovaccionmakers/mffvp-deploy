using Common.SharedKernel.Application.Reports;

namespace Reports.Application.Strategies
{
    public interface IReportStrategy
    {
        Task<ReportResponseDto> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken);

        string ReportName { get; }
        string[] ColumnHeaders { get; }
    }
}
