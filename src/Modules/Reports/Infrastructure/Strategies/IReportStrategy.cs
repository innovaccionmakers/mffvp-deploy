using Reports.Application.DTOs;

namespace Reports.Infrastructure.Strategies
{
    public interface IReportStrategy
    {
        Task<ReportResponseDto> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken);
        string ReportName { get; }
        string[] ColumnHeaders { get; }
    }
}
