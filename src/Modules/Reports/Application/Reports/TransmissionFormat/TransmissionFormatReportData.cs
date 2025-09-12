using Reports.Application.Reports.Common;

namespace Reports.Application.Reports.TransmissionFormat;

public sealed record TransmissionFormatReportData(
    decimal UnitValue,
    decimal PreviousUnits,
    decimal PreviousAmount,
    decimal YieldAmount,
    decimal PreClosingAmount,
    decimal ContributionUnits,
    decimal ContributionAmount,
    decimal TransferUnits,
    decimal TransferAmount,
    decimal PensionUnits,
    decimal PensionAmount,
    decimal WithdrawalUnits,
    decimal WithdrawalAmount,
    decimal OtherCommissionUnits,
    decimal OtherCommissionAmount,
    decimal VitalityTransferUnits,
    decimal VitalityTransferAmount,
    decimal OtherWithdrawalUnits,
    decimal OtherWithdrawalAmount,
    decimal CancellationUnits,
    decimal CancellationAmount,
    decimal CurrentUnits,
    decimal CurrentAmount,
    decimal ReturnThirtyDay,
    decimal ReturnOneHundredEightyDay,
    decimal ReturnThreeHundredSixtyFiveDay) : IReportData;

