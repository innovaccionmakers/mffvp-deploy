using Common.SharedKernel.Application.Reports;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.Services.Reports.Interfaces;
using Microsoft.AspNetCore.Mvc;
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

                if (response is FileStreamResult fileStreamResult)
                {
                    if (fileStreamResult?.FileStream == null)
                        return result;

                    using var memoryStream = new MemoryStream();

                    if (fileStreamResult.FileStream.CanSeek)
                        fileStreamResult.FileStream.Position = 0;

                    await fileStreamResult.FileStream.CopyToAsync(memoryStream);

                    var reportResponse = new ReportResponseDto
                    {
                        FileContent = Convert.ToBase64String(memoryStream.ToArray()),
                        FileName = fileStreamResult.FileDownloadName,
                        MimeType = fileStreamResult.ContentType
                    };

                    if (reportResponse != null)
                    {
                        result.Data = reportResponse;
                        return result;
                    }
                }
                else if (response is ReportResponseDto reportResponseDto)
                {
                    result.Data = reportResponseDto;
                    return result;
                }

                _logger.LogError("La respuesta del reporte es null o de tipo inesperado: {0}", response?.GetType().Name);
                result.AddError(new Error("EXCEPTION", "No se pudo generar el reporte", ErrorType.Failure));
                return result;
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
