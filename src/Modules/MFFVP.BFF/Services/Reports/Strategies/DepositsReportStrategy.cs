using ClosedXML.Excel;
using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services.Reports.DepositsReport.Interfaces;
using MFFVP.BFF.Services.Reports.Models;
using System.Diagnostics;

namespace MFFVP.BFF.Services.Reports.Strategies
{
    public class DepositsReportStrategy(IDepositsReportDataProvider _dataProvider) : IReportStrategy
    {
        public string ReportName => "Formato Mvto Manual";
        public string[] ColumnHeaders => new[]
        {
            "Tipo de Cuenta", "Número de cuenta", "Código de la transacción",
            "Fecha efectiva (AAAAMMDD)", "Valor de la transacción", "Número de cheque",
            "Naturaleza C:Crédito D:Débito", "Observaciones", "Nombre de la transacción",
            "Detalle o información adicional", "Referencia # 1 de la transacción",
            "Referencia # 2 de la transacción", "Referencia # 3 de la transacción",
            "Sucursal de la transacción"
        };

        public async Task<ReportResponseDto> GetReportDataAsync<TRequest>(
            TRequest request,
            CancellationToken cancellationToken)
        {
            if (request is DateTime processDate)
            {
                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(ReportName);

                SetupHeaders(worksheet);

                int row = 2;
                await foreach (var item in _dataProvider.GetDataAsync(processDate, cancellationToken))
                {
                    var rowData = item.ToRowData();
                    for (int col = 0; col < rowData.Length; col++)
                    {
                        var value = rowData[col];
                        var cell = worksheet.Cell(row, col + 1);

                        cell.Value = value switch
                        {
                            decimal d => Math.Round(d, 2),
                            _ => value?.ToString() ?? string.Empty
                        };

                        if (rowData[col] is decimal)
                            cell.Style.NumberFormat.Format = "0.00";
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var reportBytes = stream.ToArray();

                return new ReportResponseDto
                {
                    FileContent = Convert.ToBase64String(reportBytes),
                    FileName = $"Depositos{processDate.ToString("ddMMyyyy")}.xlsx",
                    MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };
            }
            else
            {
                throw new ArgumentException("El tipo de request no es válido. Se esperaba DateTime.");
            }
        }

        private void SetupHeaders(IXLWorksheet worksheet)
        {
            var headerStyle = worksheet.Workbook.Style;
            headerStyle.Font.Bold = true;
            headerStyle.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerStyle.Fill.BackgroundColor = XLColor.BlueGray;

            for (int i = 0; i < ColumnHeaders.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = ColumnHeaders[i];
            }
            worksheet.Range(1, 1, 1, ColumnHeaders.Length).Style = headerStyle;
        }
    }
}
