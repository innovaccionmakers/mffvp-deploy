using Common.SharedKernel.Application.Reports.Strategies;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Reports.Domain.TransmissionFormat;

namespace Reports.Application.Reports.TransmissionFormat;

public class TransmissionFormatReport(
    ILogger<TransmissionFormatReport> logger,
    ITransmissionFormatReportRepository reportRepository,
    ITransmissionFormatReportBuilder reportBuilder)
    : TextReportStrategyBase(logger)
{
    public override string ReportName => "Formato de transmision";

    public override string[] ColumnHeaders => Array.Empty<string>();

    public override async Task<IActionResult> GetReportDataAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
    {
        if (request is not TransmissionFormatReportRequest reportRequest)
        {
            logger.LogError("Tipo de request no válido. Se recibió: {RequestType}", typeof(TRequest).Name);
            throw new ArgumentException("El tipo de request no es válido.");
        }

        if (!reportRequest.IsValid())
        {
            logger.LogWarning("Request inválido recibido");
            throw new ArgumentException("El request no es válido");
        }

        try
        {
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
                var cancellationUnits = CalculateCancellationUnits(movements.CancellationAmount, unitValue);

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
                    cancellationUnits,
                    movements.CancellationAmount,
                    movements.CurrentUnits,
                    movements.CurrentAmount,
                    profitabilities.ThirtyDay,
                    profitabilities.OneHundredEightyDay,
                    profitabilities.ThreeHundredSixtyFiveDay);

                portfolioData.Add((rt2Header, data));
            }

            var content = reportBuilder.Build(rt1Header, portfolioData, reportRequest.GenerationDate);
            var fileName = $"FormatoDeTransmision_{reportRequest.GenerationDate:yyyyMMdd}.txt";

            return BuildTextFileFromContent(content, fileName);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar el reporte de formato de transmisión");
            throw;
        }
    }

    private static (decimal contributionUnits, decimal contributionAmount, decimal withdrawalUnits, decimal withdrawalAmount) ApplyAutomaticConceptAdjustment(
        decimal unitValue,
        Rt4ValuationMovements movements,
        AutomaticConceptAmounts automaticConcept)
    {
        var contributionUnits = movements.ContributionUnits;
        var contributionAmount = movements.ContributionAmount;
        var withdrawalUnits = movements.WithdrawalUnits;
        var withdrawalAmount = movements.WithdrawalAmount;

        var distributionAmount = automaticConcept.AmountToBePaid
            - (automaticConcept.AmountPaid + automaticConcept.TotalToBeDistributed);
        var notePositiveAmount = automaticConcept.PositiveToBeDistributed;
        var noteNegativeAmount = automaticConcept.NegativeToBeDistributed;

        var canAdjustUnits = unitValue > 0m;
        var contributionUnitsAdjustment = 0m;
        var withdrawalUnitsAdjustment = 0m;

        //Conceptos Automáticos: 
 
        //Cuando el concepto < 0 es aporte con signo positivo.
        //Cuando el concepto es > 0 es retiro con signo negativo.
        if (distributionAmount < 0m)
        {
            contributionAmount += Math.Abs(distributionAmount);
            if (canAdjustUnits)
            {
                contributionUnitsAdjustment += Math.Abs(distributionAmount) / unitValue;
            }
        }
        else if (distributionAmount > 0m)
        {
            withdrawalAmount += distributionAmount;
            if (canAdjustUnits)
            {
                withdrawalUnitsAdjustment += distributionAmount / unitValue;
            }
        }

        if (notePositiveAmount > 0m)
        {
            withdrawalAmount += notePositiveAmount;
            if (canAdjustUnits)
            {
                withdrawalUnitsAdjustment += notePositiveAmount / unitValue;
            }
        }

        if (noteNegativeAmount < 0m)
        {
            contributionAmount += Math.Abs(noteNegativeAmount);
            if (canAdjustUnits)
            {
                contributionUnitsAdjustment += Math.Abs(noteNegativeAmount) / unitValue;
            }
        }

        if (canAdjustUnits)
        {
            if (contributionUnitsAdjustment != 0m)
            {
                contributionUnits += Truncate(contributionUnitsAdjustment, 6);
            }

            if (withdrawalUnitsAdjustment != 0m)
            {
                withdrawalUnits += Truncate(withdrawalUnitsAdjustment, 6);
            }
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

    private static decimal CalculateCancellationUnits(decimal cancellationAmount, decimal unitValue)
    {
        if (unitValue <= 0m)
        {
            return 0m;
        }

        var absoluteAmount = Math.Abs(cancellationAmount);
        if (absoluteAmount == 0m)
        {
            return 0m;
        }

        var units = absoluteAmount / unitValue;
        return Truncate(units, 6);
    }
}
