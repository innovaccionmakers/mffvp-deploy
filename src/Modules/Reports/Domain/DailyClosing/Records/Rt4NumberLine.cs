using System;
using Common.SharedKernel.Core.Formatting;
using Reports.Domain.TransmissionFormat.Layout;

namespace Reports.Domain.TransmissionFormat.Records;

public enum SignMode
{
    PlusOnly,
    Signed,
    AlwaysNegative,
    NegativeIfNotZero
}

public sealed record Rt4NumberLine(string Prefix, string Code, decimal Value, int Decimals, SignMode SignMode)
{
    public string ToLine(int recordNumber)
    {
        var abs = Math.Abs(Value);
        var sign = SignMode switch
        {
            SignMode.PlusOnly => '+',
            SignMode.Signed => Value >= 0 ? '+' : '-',
            SignMode.AlwaysNegative => '-',
            SignMode.NegativeIfNotZero => Value != 0 ? '-' : '+',
            _ => '+'
        };
        var number = FixedWidthTextFormatter.FormatNumber(abs, 20, Decimals);
        return $"{recordNumber:00000000}{Prefix}{Code}{sign}{number}";
    }
}

public static class Rt4Lines
{
    public static Rt4NumberLine UnitValue(decimal unitValue)
        => new(TransmissionFormatLayout.Rt4.R4312, TransmissionFormatLayout.Rt4.UnitValue, unitValue, 6, SignMode.PlusOnly);

    // 4313
    public static Rt4NumberLine PreviousUnits(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.PreviousUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine PreviousAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.PreviousAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine YieldAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.YieldAmount, value, 2, SignMode.Signed);
    public static Rt4NumberLine PreClosingAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.PreClosingAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine ContributionUnits(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.ContributionUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine ContributionAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.ContributionAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine TransferUnits(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.TransferUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine TransferAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.TransferAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine PensionUnits(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.PensionUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine PensionAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.PensionAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine WithdrawalUnits(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.WithdrawalUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine WithdrawalAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.WithdrawalAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine OtherCommissionUnits(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.OtherCommissionUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine OtherCommissionAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.OtherCommissionAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine VitalityTransferUnits(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.VitalityTransferUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine VitalityTransferAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.VitalityTransferAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine OtherWithdrawalUnits(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.OtherWithdrawalUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine OtherWithdrawalAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.OtherWithdrawalAmount, value, 2, SignMode.PlusOnly);
    public static Rt4NumberLine CancellationUnits(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.CancellationUnits, value, 6, SignMode.NegativeIfNotZero);
    public static Rt4NumberLine CancellationAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.CancellationAmount, value, 2, SignMode.NegativeIfNotZero);
    public static Rt4NumberLine CurrentUnits(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.CurrentUnits, value, 6, SignMode.PlusOnly);
    public static Rt4NumberLine CurrentAmount(decimal value) => new(TransmissionFormatLayout.Rt4.R4313, TransmissionFormatLayout.Rt4.CurrentAmount, value, 2, SignMode.PlusOnly);

    // 4314
    public static Rt4NumberLine Return30Days(decimal value) => new(TransmissionFormatLayout.Rt4.R4314, TransmissionFormatLayout.Rt4.Return30Days, value, 2, SignMode.Signed);
    public static Rt4NumberLine Return180Days(decimal value) => new(TransmissionFormatLayout.Rt4.R4314, TransmissionFormatLayout.Rt4.Return180Days, value, 2, SignMode.Signed);
    public static Rt4NumberLine Return365Days(decimal value) => new(TransmissionFormatLayout.Rt4.R4314, TransmissionFormatLayout.Rt4.Return365Days, value, 2, SignMode.Signed);
}
