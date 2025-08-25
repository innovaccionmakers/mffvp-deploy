using ClosedXML.Excel;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services.Reports.DepositsReport.Interfaces;
using System.Globalization;
using Error = Common.SharedKernel.Core.Primitives.Error;

namespace MFFVP.BFF.Services.Reports.Strategies
{
    public class TechnicalSheetStrategy(
        ILogger<TechnicalSheetStrategy> _logger,
        IDepositsReportDataProvider _dataProvider
    ) : IReportStrategy
    {
        public string ReportName => "Formato ficha tecnica";

        public string[] ColumnHeaders => [
            "FECHA",
            "APORTES",
            "RETIROS",
            "PYG BRUTO",
            "GASTOS",
            "COMISIÓN DIA",
            "COSTOS DIA",
            "REND ABONADOS",
            "REND BRUTO POR UNIDAD",
            "COSTOS POR UNIDAD",
            "VR UNIDAD",
            "UNIDADES",
            "VALOR PORTAFOLIO",
            "NÚMERO PARTICIPES"   
        ];

        public async Task<GraphqlResult<ReportResponseDto>> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        {
            var result = new GraphqlResult<ReportResponseDto>();

            try
            {
                if (request is not DateTime processDate)
                {
                    _logger.LogError($"Tipo de request no válido. Se esperaba DateTime, se recibió: {request}", typeof(TRequest).Name);
                    result.AddError(new Error("EXCEPTION", "El tipo de request no es válido. Se esperaba DateTime.", ErrorType.Failure));
                    return result;
                }

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(ReportName);
                SetupHeaders(worksheet);

                int row = 2;
                await foreach (var item in _dataProvider.GetDataAsync(processDate, cancellationToken))
                {
                    if (!item.Success)
                    {
                        foreach (var error in item.Errors)
                        {
                            _logger.LogError($"Error: {error.Code} - {error.Description}");
                            result.AddError(new Error($"{error.Code}", $"{error.Description}", ErrorType.Failure));
                            return result;
                        }
                    }

                    var rowData = item.Data.ToRowData();
                    for (int col = 0; col < rowData.Length; col++)
                    {
                        var value = rowData[col];
                        var cell = worksheet.Cell(row, col + 1);

                        cell.Value = value switch
                        {
                            decimal d => d.ToString("0.00", CultureInfo.InvariantCulture),
                            _ => value?.ToString() ?? string.Empty
                        };
                    }
                    row++;
                }

                worksheet.Columns().AdjustToContents();

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);
                var reportBytes = stream.ToArray();

                var response = new ReportResponseDto
                {
                    FileContent = Convert.ToBase64String(reportBytes),
                    FileName = $"ThechnicalSheet{processDate.ToString("ddMMyyyy")}.xlsx",
                    MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };

                result.SetSuccess(response);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Tipo de request no válido. Se esperaba DateTime, se recibió: {request}", typeof(TRequest).Name);
                result.AddError(new Error("EXCEPTION", "El tipo de request no es válido. Se esperaba DateTime.", ErrorType.Failure));
                return result;
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
