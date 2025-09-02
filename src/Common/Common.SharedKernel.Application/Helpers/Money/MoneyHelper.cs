
using Common.SharedKernel.Application.Constants;

namespace Common.SharedKernel.Application.Helpers.Money;

public static class MoneyHelper
{
    public static decimal Round2(decimal value) =>
        Math.Round(value, DecimalPrecision.TwoDecimals, MidpointRounding.AwayFromZero);

    public static decimal RoundToScale(decimal value, int scale = DecimalPrecision.TwoDecimals) =>
        Math.Round(value, scale, MidpointRounding.AwayFromZero);
}