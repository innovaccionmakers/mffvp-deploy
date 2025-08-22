using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services.Reports.Interfaces;

namespace MFFVP.BFF.Services.Reports
{
    public class ReportOrchestrator(
        IExcelReportService _excelReportService,
        ILogger<ReportOrchestrator> _logger)
    {
        public async Task<ReportResponseDto> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _excelReportService.GetReportDataAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en la orquestación del reporte");
                throw;
            }
        }
    }
}
