using Common.SharedKernel.Application.Constants;

namespace Common.SharedKernel.Application.Helpers.Money;

/// <summary>
/// Redondea un valor decimal a X lugares usando MidpointRounding.AwayFromZero.
/// </summary>
public static class MoneyHelper
{
    ///<summary>
    ///Redondea un valor decimal a dos lugares usando MidpointRounding.AwayFromZero.
    ///</summary>
    public static decimal Round2(decimal value) =>
        Math.Round(value, DecimalPrecision.TwoDecimals, MidpointRounding.AwayFromZero);

    ///<summary>
    ///Redondea un valor decimal a los lugares definidos en scale usando MidpointRounding.AwayFromZero.
    ///</summary>
    public static decimal RoundToScale(decimal value, int scale = DecimalPrecision.TwoDecimals) =>
        Math.Round(value, scale, MidpointRounding.AwayFromZero);
}