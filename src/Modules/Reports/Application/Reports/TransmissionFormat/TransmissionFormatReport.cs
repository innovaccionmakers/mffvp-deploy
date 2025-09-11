using Microsoft.Extensions.Logging;
using Reports.Application.Strategies;
using Reports.Domain.TransmissionFormat;

namespace Reports.Application.Reports.TransmissionFormat;

public class TransmissionFormatReport(
    ILogger<TransmissionFormatReport> logger,
    ITransmissionFormatReportRepository reportRepository,
    ITransmissionFormatReportBuilder reportBuilder)
    : TextReportStrategyBase<TransmissionFormatReport>(logger)
{
    public override string ReportName => "Formato de transmision";

    protected override async Task<string> GenerateReportContentAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        if (request is not TransmissionFormatReportRequest reportRequest)
        {
            Logger.LogError("Tipo de request no válido. Se recibió: {RequestType}", typeof(TRequest).Name);
            throw new ArgumentException("El tipo de request no es válido.");
        }

        if (!reportRequest.IsValid())
        {
            Logger.LogWarning("Request inválido recibido");
            throw new ArgumentException("El request no es válido");
        }

        var existsAtDate = await reportRepository.AnyPortfolioExistsOnOrAfterDateAsync(reportRequest.GenerationDate, cancellationToken);
        if (!existsAtDate)
        {
            throw new ArgumentException("Ningun portafolio existía en la fecha ingresada.");
        }

        var portfolioIds = await reportRepository.GetPortfolioIdsWithClosureOnDateAsync(reportRequest.GenerationDate, cancellationToken);
        if (portfolioIds is null || portfolioIds.Count == 0)
        {
            throw new ArgumentException("No hay información disponible.");
        }

        var rt1Header = await reportRepository.GetRt1HeaderAsync(portfolioIds[0], cancellationToken);

        var portfolioData = new List<(Rt2Header header, TransmissionFormatReportData data)>();
        foreach (var pid in portfolioIds)
        {
            var rt2Header = await reportRepository.GetRt2HeaderAsync(pid, cancellationToken);
            var unitValue = await reportRepository.GetUnitValueAsync(pid, reportRequest.GenerationDate, cancellationToken);
            var movements = await reportRepository.GetValuationMovementsAsync(pid, reportRequest.GenerationDate, cancellationToken);
            var profitabilities = await reportRepository.GetProfitabilitiesAsync(pid, reportRequest.GenerationDate, cancellationToken);

            var preClosing = movements.PreviousAmount + movements.YieldAmount;
            var data = new TransmissionFormatReportData(
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

            portfolioData.Add((rt2Header, data));
        }

        return reportBuilder.Build(rt1Header, portfolioData, reportRequest.GenerationDate);
    }

    protected override string GenerateFileName(object request) =>
        request is TransmissionFormatReportRequest reportRequest
            ? $"FormatoDeTransmision_{reportRequest.GenerationDate:yyyyMMdd}.txt"
            : base.GenerateFileName(request);
}
