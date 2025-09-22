using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Reports.Application.Reports.Strategies;
using Reports.Domain.BalancesAndMovements;
using Reports.Domain.TechnicalSheet;

namespace Reports.Application.TechnicalSheet;

public class TechnicalSheetReport(
        ILogger<TechnicalSheetReport> logger,
        ITechnicalSheetRepository technicalSheetRepository) : ExcelReportStrategyBase(logger)
{
    public override string ReportName => "InformaciónFichaTécnica";

    public override string[] ColumnHeaders =>
    [
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

    public override async Task<IActionResult> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        if(request is TechnicalSheetReportRequest reportRequest)
        {
            if (!reportRequest.IsValid())
            {
                var validationErrors = reportRequest.GetValidationErrors();
                var firstError = validationErrors.FirstOrDefault() ?? "El request proporcionado no es válido";
                logger.LogError("Request inválido: {Request}. Error: {ValidationError}", reportRequest, firstError);
                throw new ArgumentException(firstError);
            }
            try
            {
                return await GenerateExcelReportAsync(
                    ct => GetWorksheetDataAsync(reportRequest, ct),
                    GenerateReportFileName(reportRequest),
                    cancellationToken);
            }
            catch(Exception ex)
            {

                logger.LogError(ex, "Error al generar el reporte de ficha técnica para el request: {Request}", reportRequest);
                throw;
            }
        }
        else
        {
            logger.LogError("Tipo de request no válido. Se esperaba DateTime, se recibió: {RequestType}",
                typeof(TRequest).Name);
            throw new ArgumentException("El tipo de request no es válido. Se esperaba DateTime.");
        }
    }

    private async Task<List<WorksheetData>> GetWorksheetDataAsync(TechnicalSheetReportRequest reportRequest, CancellationToken cancellationToken)
    {
        var worksheetDataList = new List<WorksheetData>();

        var data = await technicalSheetRepository.GetByDateRangeAndPortfolioAsync(reportRequest.StartDate, reportRequest.EndDate, reportRequest.PortfolioId, cancellationToken);

        var rows = data?.Select(x => new object[]
        {
            x.Date,
            x.Contributions,
            x.Withdrawals,
            x.GrossPnl,
            x.Expenses,
            x.DailyCommission,
            x.DailyCost,
            x.CreditedYields,
            x.GrossUnitYield,
            x.UnitCost,
            x.UnitValue,
            x.Units,
            x.PortfolioValue,
            x.Participants
        }).ToList();

        var saldosData = new WorksheetData
        {
            WorksheetName = WorksheetName.TechnicalSheet.GetDescription(),
            ColumnHeaders = ColumnHeaders,
            Rows = rows ?? []
        };
        worksheetDataList.Add(saldosData);


        return await Task.FromResult(worksheetDataList);
    }

    private string GenerateReportFileName(TechnicalSheetReportRequest reportRequest)
    {
        var startDate = reportRequest.StartDate.ToString("ddMMyyyy");
        var endDate = reportRequest.EndDate.ToString("ddMMyyyy");
        return $"{ReportName}_{startDate}_{endDate}_Portafolio{reportRequest.PortfolioId}.xlsx";
    }
}
