using Common.SharedKernel.Application.Reports;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.Services.Reports.Interfaces;
using Reports.Presentation.GraphQL;
using Error = Common.SharedKernel.Core.Primitives.Error;

namespace MFFVP.BFF.Services.Reports
{
    public class ReportOrchestrator(
        IReportsExperienceQueries _reportsExperience,
        IExcelReportService _excelReportService,
        ILogger<ReportOrchestrator> _logger)
    {
        public async Task<GraphqlResult<ReportResponseDto>> GetReportDataAsync<TRequest>(TRequest request, ReportType reportType, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<ReportResponseDto>();
            try
            {
                return await _excelReportService.GetReportDataAsync(request, reportType, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en la orquestación del reporte");
                result.AddError(new Error("EXCEPTION", "Error en la orquestación del reporte", ErrorType.Failure));
                return result;
            }
        }

        public async Task<GraphqlResult<ReportResponseDto>> GetReportData<TRequest>(TRequest request, ReportType reportType, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<ReportResponseDto>();
            try
            { 
                var response = await _reportsExperience.GetReportDataAsync(request, reportType, cancellationToken);
                if (response != null)
                {
                    result.Data = response;
                    return result;
                }
                else
                {
                    _logger.LogError("La respuesta del reporte es nula");
                    result.AddError(new Error("NULL_RESPONSE", "La respuesta del reporte es nula", ErrorType.Failure));
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la orquestación del reporte");
                result.AddError(new Error("EXCEPTION", "Error en la orquestación del reporte", ErrorType.Failure));
                return result;
            }
        }
    }
}
