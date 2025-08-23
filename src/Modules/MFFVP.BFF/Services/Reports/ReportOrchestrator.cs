using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services.Reports.Interfaces;
using Error = Common.SharedKernel.Core.Primitives.Error;

namespace MFFVP.BFF.Services.Reports
{
    public class ReportOrchestrator(
        IExcelReportService _excelReportService,
        ILogger<ReportOrchestrator> _logger)
    {
        public async Task<GraphqlResult<ReportResponseDto>> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<ReportResponseDto>();
            try
            {
                return await _excelReportService.GetReportDataAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en la orquestación del reporte");
                result.AddError(new Error("EXCEPTION", "Error en la orquestación del reporte", ErrorType.Failure));
                return result;
            }
        }
    }
}
