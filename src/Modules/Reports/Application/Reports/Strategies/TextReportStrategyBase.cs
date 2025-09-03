using System.Text;
using Common.SharedKernel.Application.Reports;
using Microsoft.Extensions.Logging;
using Reports.Application.Reports.Strategies;

namespace Reports.Application.Strategies;

public abstract class TextReportStrategyBase<TStrategy>(ILogger<TStrategy> logger) : IReportStrategy where TStrategy : class
{
    protected readonly ILogger<TStrategy> Logger = logger;

    public abstract string ReportName { get; }

    public virtual string[] ColumnHeaders => Array.Empty<string>();

    protected virtual string GenerateFileName(object request) => $"{ReportName}.txt";

    protected abstract Task<string> GenerateReportContentAsync<TRequest>(TRequest request, CancellationToken cancellationToken);

    public async Task<ReportResponseDto> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var content = await GenerateReportContentAsync(request, cancellationToken);
            var bytes = Encoding.UTF8.GetBytes(content);
            return new ReportResponseDto
            {
                FileContent = Convert.ToBase64String(bytes),
                FileName = GenerateFileName(request),
                MimeType = "text/plain"
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error al generar el reporte de texto");
            throw new Exception($"Error al generar el reporte: {ex.Message}", ex);
        }
    }
}
