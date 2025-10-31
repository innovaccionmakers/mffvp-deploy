using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;

namespace Common.SharedKernel.Application.Reports.Strategies;

public abstract class TextReportStrategyBase(
    ILogger<TextReportStrategyBase> logger) : IReportStrategy
{
    public abstract string ReportName { get; }
    public abstract string[] ColumnHeaders { get; }

    protected virtual string FormatValue(object value)
    {
        var formattedValue = value switch
        {
            decimal d => d.ToString("0.00", CultureInfo.InvariantCulture),
            double doubleVal => doubleVal.ToString("0.00", CultureInfo.InvariantCulture),
            float floatVal => floatVal.ToString("0.00", CultureInfo.InvariantCulture),
            DateTime dateTime => dateTime.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
            _ => value?.ToString() ?? string.Empty
        };

        return NormalizeText(formattedValue);
    }

    protected virtual string GetFieldSeparator() => "|";

    protected virtual string GetRecordSeparator() => Environment.NewLine;


    protected virtual string NormalizeText(string text)
    {
        return text;
    }

    protected string RemoveAccentsAndNormalize(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                if (c == 'ñ' || c == 'Ñ')
                {
                    stringBuilder.Append(c == 'ñ' ? 'n' : 'N');
                }
                else
                {
                    stringBuilder.Append(c);
                }
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    protected virtual Encoding GetEncoding() => Encoding.UTF8;

    protected virtual async Task<FileStreamResult> GenerateTextReportAsync(
        Func<CancellationToken, Task<List<TextReportData>>> dataProvider,
        string fileName,
        CancellationToken cancellationToken)
    {
        try
        {
            var textReportDataList = await dataProvider(cancellationToken);
            return BuildTextFile(textReportDataList, fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar el reporte de texto");
            throw new Exception($"Error al generar el reporte: {ex.Message}", ex);
        }
    }

    protected virtual Task<FileStreamResult> GenerateTextReportAsync(
        List<TextReportData> textReportDataList,
        string fileName,
        CancellationToken cancellationToken)
    {
        try
        {
            return Task.FromResult(BuildTextFile(textReportDataList, fileName));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar el reporte de texto (datos externos)");
            throw new Exception($"Error al generar el reporte: {ex.Message}", ex);
        }
    }

    protected virtual FileStreamResult BuildTextFileFromContent(string content, string fileName)
    {
        var encoding = GetEncoding();
        var bytes = encoding.GetBytes(content);
        var memoryStream = new MemoryStream(bytes);

        return new FileStreamResult(memoryStream, "text/plain")
        {
            FileDownloadName = fileName
        };
    }

    private FileStreamResult BuildTextFile(List<TextReportData> textReportDataList, string fileName)
    {
        var stringBuilder = new StringBuilder();
        var separator = GetFieldSeparator();
        var recordSeparator = GetRecordSeparator();

        foreach (var textReportData in textReportDataList)
        {
            if (!string.IsNullOrWhiteSpace(textReportData.SectionTitle))
            {
                stringBuilder.AppendLine(NormalizeText(textReportData.SectionTitle));
            }

            if (textReportData.IncludeHeaders && textReportData.ColumnHeaders.Length > 0)
            {
                var normalizedHeaders = textReportData.ColumnHeaders.Select(NormalizeText).ToArray();
                stringBuilder.AppendLine(string.Join(separator, normalizedHeaders));
            }

            foreach (var dataRow in textReportData.Rows)
            {
                var formattedValues = dataRow.Select(FormatValue).ToArray();
                stringBuilder.Append(string.Join(separator, formattedValues));
                stringBuilder.Append(recordSeparator);
            }

            if (textReportDataList.Count > 1 && textReportData != textReportDataList.Last())
            {
                stringBuilder.AppendLine();
            }
        }

        var content = stringBuilder.ToString();
        return BuildTextFileFromContent(content, fileName);
    }

    public abstract Task<IActionResult> GetReportDataAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken);
}

public class TextReportData
{
    public string SectionTitle { get; set; } = string.Empty;
    public string[] ColumnHeaders { get; set; } = Array.Empty<string>();
    public bool IncludeHeaders { get; set; } = true;
    public List<object[]> Rows { get; set; } = new List<object[]>();
}
