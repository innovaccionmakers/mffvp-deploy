using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services.Reports.Interfaces;

namespace MFFVP.BFF.Services.Reports
{
    public class ReportOrchestrator
    {
        private readonly IExcelReportService _excelReportService;

        public ReportOrchestrator(IExcelReportService excelReportService)
        {
            _excelReportService = excelReportService;
        }

        public async Task<ReportResponseDto> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        {
            return await _excelReportService.GetReportDataAsync(request, cancellationToken);
        }
    }
}
