using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Common.SharedKernel.Application.Reports.Strategies;

public abstract class ExcelReportStrategyBase(
    ILogger<ExcelReportStrategyBase> logger) : IReportStrategy
{
    public abstract string ReportName { get; }
    public abstract string[] ColumnHeaders { get; }

    protected virtual void ApplyCellFormatting(IXLCell cell, object value)
    {
        cell.Value = value switch
        {
            decimal d => d.ToString("0.00", CultureInfo.InvariantCulture),
            double doubleVal => doubleVal.ToString("0.00", CultureInfo.InvariantCulture),
            float floatVal => floatVal.ToString("0.00", CultureInfo.InvariantCulture),
            _ => value?.ToString() ?? string.Empty
        };
    }

    protected virtual void SetupHeaders(IXLWorksheet worksheet, string[] columnHeaders)
    {
        for (int i = 0; i < columnHeaders.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = columnHeaders[i];
            cell.Style.Font.Bold = true;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Fill.BackgroundColor = XLColor.BlueGray;
            cell.Style.Fill.PatternType = XLFillPatternValues.Solid;
        }

        worksheet.SheetView.FreezeRows(1);
    }

    protected virtual async Task<FileStreamResult> GenerateExcelReportAsync(
        Func<CancellationToken, Task<List<WorksheetData>>> dataProvider,
        string fileName,
        CancellationToken cancellationToken)
    {
        try
        {
            using var workbook = new XLWorkbook();

            var worksheetDataList = await dataProvider(cancellationToken);

            foreach (var worksheetData in worksheetDataList)
            {
                var worksheet = workbook.Worksheets.Add(worksheetData.WorksheetName);

                SetupHeaders(worksheet, worksheetData.ColumnHeaders);

                int row = 2;
                foreach (var dataRow in worksheetData.Rows)
                {
                    for (int col = 0; col < dataRow.Length; col++)
                    {
                        var value = dataRow[col];
                        var cell = worksheet.Cell(row, col + 1);
                        ApplyCellFormatting(cell, value);
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();
            }

            using var memoryStream = new MemoryStream();
            workbook.SaveAs(memoryStream);
            memoryStream.Position = 0;
            var fileBytes = memoryStream.ToArray();

            return new FileStreamResult(new MemoryStream(fileBytes), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                FileDownloadName = fileName
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar el reporte Excel");
            throw new Exception($"Error al generar el reporte: {ex.Message}", ex);
        }
    }

    public abstract Task<IActionResult> GetReportDataAsync<TRequest>(
        TRequest request,
        CancellationToken cancellationToken);
}

public class WorksheetData
{
    public string WorksheetName { get; set; } = string.Empty;
    public string[] ColumnHeaders { get; set; } = Array.Empty<string>();
    public List<object[]> Rows { get; set; } = new List<object[]>();
}
