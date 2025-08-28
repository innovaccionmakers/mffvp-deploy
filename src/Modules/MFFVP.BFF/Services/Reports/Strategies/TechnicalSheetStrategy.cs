using ClosedXML.Excel;
using Common.SharedKernel.Application.Reports;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.DTOs;
using MFFVP.BFF.Services.Reports.TechnicalSheet.Interfaces;
using System.Globalization;
using Error = Common.SharedKernel.Core.Primitives.Error;

namespace MFFVP.BFF.Services.Reports.Strategies
{
    public class TechnicalSheetStrategy(
        ILogger<TechnicalSheetStrategy> _logger,
        ITechnicalSheetReportDataProvider _dataProvider
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
                TechnicalSheetRequestDto technicalSheetRequest;

                if (request is TechnicalSheetRequestDto dto)
                {
                    technicalSheetRequest = dto;
                }
                else if (request is ValueTuple<int, DateOnly, DateOnly> tuple)
                {
                    technicalSheetRequest = new TechnicalSheetRequestDto
                    {
                        PortfolioId = tuple.Item1,
                        StartDate = tuple.Item2,
                        EndDate = tuple.Item3
                    };
                }
                else
                {
                    _logger.LogError($"Tipo de request no válido. Se esperaba TechnicalSheetRequestDto o (int, DateOnly, DateOnly), se recibió: {typeof(TRequest).Name}");
                    result.AddError(new Error("EXCEPTION", "El tipo de request no es válido. Se esperaba TechnicalSheetRequestDto o (int, DateOnly, DateOnly).", ErrorType.Failure));
                    return result;
                }

                
                if (technicalSheetRequest.StartDate > technicalSheetRequest.EndDate)
                {
                    _logger.LogError($"Fecha de inicio ({technicalSheetRequest.StartDate}) no puede ser mayor que la fecha de fin ({technicalSheetRequest.EndDate})");
                    result.AddError(new Error("VALIDATION_ERROR", "La fecha de inicio no puede ser mayor que la fecha de fin.", ErrorType.Failure));
                    return result;
                }

                if (technicalSheetRequest.PortfolioId <= 0)
                {
                    _logger.LogError($"PortfolioId debe ser mayor que 0, se recibió: {technicalSheetRequest.PortfolioId}");
                    result.AddError(new Error("VALIDATION_ERROR", "El PortfolioId debe ser mayor que 0.", ErrorType.Failure));
                    return result;
                }

                using var workbook = new XLWorkbook();
                var worksheet = workbook.Worksheets.Add(ReportName);
                SetupHeaders(worksheet);

                int row = 2;
                await foreach (var item in _dataProvider.GetDataAsync(technicalSheetRequest.StartDate, technicalSheetRequest.EndDate, technicalSheetRequest.PortfolioId, cancellationToken))
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
                    FileName = $"TechnicalSheet_{technicalSheetRequest.StartDate:ddMMyyyy}_{technicalSheetRequest.EndDate:ddMMyyyy}_Portfolio{technicalSheetRequest.PortfolioId}.xlsx",
                    MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
                };

                result.SetSuccess(response);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error inesperado al generar reporte: {ex.Message}", ex);
                result.AddError(new Error("EXCEPTION", "Error inesperado al generar el reporte.", ErrorType.Failure));
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
