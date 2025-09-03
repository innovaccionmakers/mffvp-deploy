using Microsoft.Extensions.Logging;
using Reports.Application.Strategies;
using Reports.Domain.DailyClosing;

namespace Reports.Application.Reports.DailyClosing;

public class DailyClosingReport(
    ILogger<DailyClosingReport> logger,
    IDailyClosingReportRepository reportRepository,
    IDailyClosingReportBuilder reportBuilder)
    : TextReportStrategyBase<DailyClosingReport>(logger)
{
    public override string ReportName => "Daily Closing Report";

    protected override async Task<string> GenerateReportContentAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        if (request is not DailyClosingReportRequest reportRequest)
        {
            Logger.LogError("Tipo de request no válido. Se recibió: {RequestType}", typeof(TRequest).Name);
            throw new ArgumentException("El tipo de request no es válido.");
        }

        if (!reportRequest.IsValid())
        {
            Logger.LogWarning("Request inválido recibido");
            throw new ArgumentException("El request no es válido");
        }

        var rt1Header = await reportRepository.GetRt1HeaderAsync(reportRequest.PortfolioId, cancellationToken);
        var rt2Header = await reportRepository.GetRt2HeaderAsync(reportRequest.PortfolioId, cancellationToken);
        var unitValue = await reportRepository.GetUnitValueAsync(reportRequest.PortfolioId, reportRequest.GenerationDate, cancellationToken);
        var movements = await reportRepository.GetValuationMovementsAsync(reportRequest.PortfolioId, reportRequest.GenerationDate, cancellationToken);
        var profitabilities = await reportRepository.GetProfitabilitiesAsync(reportRequest.PortfolioId, reportRequest.GenerationDate, cancellationToken);
        var preClosing = movements.PreviousAmount + movements.YieldAmount;
        var data = new DailyReportData(
            unitValue,
            movements.PreviousUnits,
            movements.PreviousAmount,
            movements.YieldAmount,
            preClosing,
            movements.ContributionUnits,
            movements.ContributionAmount,
            movements.TransferUnits,
            movements.TransferAmount,
            movements.PensionUnits,
            movements.PensionAmount,
            movements.WithdrawalUnits,
            movements.WithdrawalAmount,
            movements.OtherCommissionUnits,
            movements.OtherCommissionAmount,
            movements.VitalityTransferUnits,
            movements.VitalityTransferAmount,
            movements.OtherWithdrawalUnits,
            movements.OtherWithdrawalAmount,
            movements.CancellationUnits,
            movements.CancellationAmount,
            movements.CurrentUnits,
            movements.CurrentAmount,
            profitabilities.ThirtyDay,
            profitabilities.OneHundredEightyDay,
            profitabilities.ThreeHundredSixtyFiveDay);

        return reportBuilder.Build(rt1Header, rt2Header, data, reportRequest.GenerationDate);
    }

    protected override string GenerateFileName(object request) =>
        request is DailyClosingReportRequest reportRequest
            ? $"DailyClosing_{reportRequest.GenerationDate:yyyyMMdd}.txt"
            : base.GenerateFileName(request);
}
