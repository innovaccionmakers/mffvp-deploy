using Common.SharedKernel.Application.Reports;

namespace Reports.Presentation.GraphQL
{
    public interface IReportsExperienceQueries
    {
        Task<ReportResponseDto> GetReportDataAsync<TRequest>(TRequest request, ReportType reportType, CancellationToken cancellationToken = default);
    }
}
