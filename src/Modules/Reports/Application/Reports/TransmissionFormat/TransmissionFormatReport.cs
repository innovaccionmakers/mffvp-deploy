using Common.SharedKernel.Application.Reports.Strategies;
using Microsoft.Extensions.Logging;
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
            throw new ArgumentException("Ningun portafolio existe en la fecha ingresada.");
        }

        var portfolioIds = await reportRepository.GetPortfolioIdsWithClosureOnDateAsync(reportRequest.GenerationDate, cancellationToken);
        if (portfolioIds is null || portfolioIds.Count == 0)
        {
            throw new ArgumentException("No hay información disponible para la fecha ingresada.");
        }

        var rt1Header = await reportRepository.GetRt1HeaderAsync(portfolioIds[0], cancellationToken);

        var portfolioData = new List<(Rt2Header header, TransmissionFormatReportData data)>();
        foreach (var pid in portfolioIds)
        {
            var rt2Header = await reportRepository.GetRt2HeaderAsync(pid, cancellationToken);
            var unitValue = await reportRepository.GetUnitValueAsync(pid, reportRequest.GenerationDate, cancellationToken);
            var movements = await reportRepository.GetValuationMovementsAsync(pid, reportRequest.GenerationDate, cancellationToken);
            var profitabilities = await reportRepository.GetProfitabilitiesAsync(pid, reportRequest.GenerationDate, cancellationToken);
            var automaticConcept = await reportRepository.GetAutomaticConceptAmountsAsync(pid, reportRequest.GenerationDate, cancellationToken);

            var preClosing = movements.PreviousAmount + movements.YieldAmount;
            var (contributionUnits, contributionAmount, withdrawalUnits, withdrawalAmount) = ApplyAutomaticConceptAdjustment(
                unitValue,
                movements,
                automaticConcept);

            var data = new TransmissionFormatReportData(
                unitValue,
                movements.PreviousUnits,
                movements.PreviousAmount,
                movements.YieldAmount,
                preClosing,
                contributionUnits,
                contributionAmount,
                movements.TransferUnits,
                movements.TransferAmount,
                movements.PensionUnits,
                movements.PensionAmount,
                withdrawalUnits,
                withdrawalAmount,
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

    private static (decimal contributionUnits, decimal contributionAmount, decimal withdrawalUnits, decimal withdrawalAmount) ApplyAutomaticConceptAdjustment(
        decimal unitValue,
        Rt4ValuationMovements movements,
        AutomaticConceptAmounts automaticConcept)
    {
        var contributionUnits = movements.ContributionUnits;
        var contributionAmount = movements.ContributionAmount;
        var withdrawalUnits = movements.WithdrawalUnits;
        var withdrawalAmount = movements.WithdrawalAmount;

        var conceptDifference = automaticConcept.AmountToBePaid - automaticConcept.AmountPaid;
        if (conceptDifference == 0m)
        {
            return (contributionUnits, contributionAmount, withdrawalUnits, withdrawalAmount);
        }

        var absoluteDifference = Math.Abs(conceptDifference);
        var unitsAdjustment = unitValue <= 0m ? 0m : Truncate(absoluteDifference / unitValue, 6);

        if (conceptDifference < 0m)
        {
            contributionUnits += unitsAdjustment;
            contributionAmount += absoluteDifference;
        }
        else
        {
            withdrawalUnits = -unitsAdjustment;
            withdrawalAmount = -absoluteDifference;
        }

        return (contributionUnits, contributionAmount, withdrawalUnits, withdrawalAmount);
    }

    private static decimal Truncate(decimal value, int decimals)
    {
        var factor = 1m;
        for (var i = 0; i < decimals; i++)
        {
            factor *= 10m;
        }

        return Math.Truncate(value * factor) / factor;
    }
}
