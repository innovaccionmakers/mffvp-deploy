using ClosedXML.Excel;
using Common.SharedKernel.Application.Reports;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Reports.Application.Strategies
{
    public abstract class ExcelReportStrategyBase(
        ILogger<ExcelReportStrategyBase> logger) : IReportStrategy
    {
        public abstract string ReportName { get; }
        public abstract string[] ColumnHeaders { get; }

        protected virtual string GenerateFileName(object request) => $"{ReportName}.xlsx";

        protected virtual void ApplyCellFormatting(IXLCell cell, object value)
        {
            cell.Value = value switch
            {
                decimal d => d.ToString("0.00", CultureInfo.InvariantCulture),
                double doubleVal => doubleVal.ToString("0.00", CultureInfo.InvariantCulture),
                float floatVal => floatVal.ToString("0.00", CultureInfo.InvariantCulture),
                DateTime date => date.ToString("yyyyMMdd"),
                DateOnly dateOnly => dateOnly.ToString("yyyyMMdd"),
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

        protected virtual async Task<ReportResponseDto> GenerateExcelReportAsync(
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

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var reportBytes = stream.ToArray();

                return new ReportResponseDto
                {
                    FileContent = Convert.ToBase64String(reportBytes),
                    FileName = fileName,
                    MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error al generar el reporte Excel");
                throw new Exception($"Error al generar el reporte: {ex.Message}", ex);
            }
        }

        public abstract Task<ReportResponseDto> GetReportDataAsync<TRequest>(
            TRequest request,
            CancellationToken cancellationToken);
    }

    public class WorksheetData
    {
        public string WorksheetName { get; set; } = string.Empty;
        public string[] ColumnHeaders { get; set; } = Array.Empty<string>();
        public List<object[]> Rows { get; set; } = new List<object[]>();
    }
}
