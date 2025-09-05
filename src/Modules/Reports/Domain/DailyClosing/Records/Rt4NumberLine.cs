using System;
using Common.SharedKernel.Core.Formatting;
using Reports.Domain.DailyClosing.Layout;

namespace Reports.Domain.DailyClosing.Records;

public enum SignMode
{
    PlusOnly,
    Signed
}

public sealed record Rt4NumberLine(string Prefix, string Code, decimal Value, int Decimals, SignMode SignMode)
{
    public string ToLine(int recordNumber)
    {
        var abs = Math.Abs(Value);
        var sign = SignMode == SignMode.PlusOnly ? '+' : (Value >= 0 ? '+' : '-');
        var number = FixedWidthTextFormatter.FormatNumber(abs, 20, Decimals);
        return $"{recordNumber:00000}{Prefix}{Code}{sign}{number}";
    }
}

public static class Rt4Lines
{
    public static Rt4NumberLine UnitValue(decimal unitValue)
        => new(DailyClosingLayout.Rt4.R4312, DailyClosingLayout.Rt4.UnitValue, unitValue, 6, SignMode.PlusOnly);

    // 4313
    public static Rt4NumberLine PreviousUnits(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.PreviousUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine PreviousAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.PreviousAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine YieldAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.YieldAmount, value, 2, SignMode.Signed);
    public static Rt4NumberLine PreClosingAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.PreClosingAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine ContributionUnits(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.ContributionUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine ContributionAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.ContributionAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine TransferUnits(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.TransferUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine TransferAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.TransferAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine PensionUnits(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.PensionUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine PensionAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.PensionAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine WithdrawalUnits(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.WithdrawalUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine WithdrawalAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.WithdrawalAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine OtherCommissionUnits(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.OtherCommissionUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine OtherCommissionAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.OtherCommissionAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine VitalityTransferUnits(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.VitalityTransferUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine VitalityTransferAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.VitalityTransferAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine OtherWithdrawalUnits(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.OtherWithdrawalUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine OtherWithdrawalAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.OtherWithdrawalAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine CancellationUnits(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.CancellationUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine CancellationAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.CancellationAmount, value, 2, SignMode.Signed);
    public static Rt4NumberLine CurrentUnits(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.CurrentUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine CurrentAmount(decimal value) => new(DailyClosingLayout.Rt4.R4313, DailyClosingLayout.Rt4.CurrentAmount, value, 2, SignMode.PlusOnly);

    // 4314
    public static Rt4NumberLine Return30Days(decimal value) => new(DailyClosingLayout.Rt4.R4314, DailyClosingLayout.Rt4.Return30Days, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine Return180Days(decimal value) => new(DailyClosingLayout.Rt4.R4314, DailyClosingLayout.Rt4.Return180Days, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine Return365Days(decimal value) => new(DailyClosingLayout.Rt4.R4314, DailyClosingLayout.Rt4.Return365Days, value, 2, SignMode.PlusOnly);
}
