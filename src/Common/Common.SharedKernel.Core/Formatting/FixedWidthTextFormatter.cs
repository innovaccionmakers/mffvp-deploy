using System;
using System.Globalization;

namespace Common.SharedKernel.Core.Formatting;

public static class FixedWidthTextFormatter
{
    public static string FormatNumber(decimal value, int length, int decimals)
    {
        var factor = (decimal)Math.Pow(10, decimals);
        var truncated = Math.Truncate(value * factor) / factor;
        var format = "F" + decimals;
        var text = truncated.ToString(format, CultureInfo.InvariantCulture);
        return text.PadLeft(length, '0');
    }

    public static string FormatNumberStrict(decimal value, int length, int decimals)
    {
        var text = FormatNumber(value, length, decimals);
        if (text.Length > length)
        {
            throw new OverflowException($"Value {value} exceeds fixed length {length} with {decimals} decimals.");
        }
        return text;
    }

    public static char GetSign(decimal value) => value >= 0 ? '+' : '-';

    public static string FormatDate(DateTime date)
        => date.ToString("ddMMyyyy", CultureInfo.InvariantCulture);
}

