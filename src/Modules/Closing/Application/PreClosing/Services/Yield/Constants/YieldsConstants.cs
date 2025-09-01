namespace Closing.Application.PreClosing.Services.Yield.Constants;
public struct YieldsSources
{
    public const string
        ProfitAndLoss = "CargaExterna",
        Commission = "Comisión",
        Treasury = "Tesorería",
        AutomaticConcept = "Concepto Automático"
        ;
}

public enum PersistenceMode { Immediate, Transactional }

/// <summary>
/// Límites matemáticos para cálculos de rentabilidad/unidad que evitan overflow en decimal.
/// - OverflowDailyGrowthMax  ≈ (decimal.MaxValue)^(1/365)
/// - OverflowYieldFraction   = OverflowDailyGrowthMax - 1  ≈ 0.1999827763558086 (≈20%)
/// - OverflowSafeYieldFraction: margen seguro por debajo del límite teórico (p.ej. 19%)
/// 
/// Nota: las fracciones aquí están en términos de "parte del monto previo", no en %.
/// </summary>
public static class YieldMathLimits
{
    /// <summary>Máximo factor de crecimiento diario permitido sin overflow al elevar a 365.</summary>
    public const decimal OverflowDailyGrowthMax = 1.1999827763558086m;

    /// <summary>Fracción máxima de yield respecto al monto previo (≈ 20%).</summary>
    public const decimal OverflowYieldFraction = 0.1999827763558086m; // = OverflowDailyGrowthMax - 1m

    /// <summary>Fracción segura recomendada por debajo del límite teórico (p.ej., 19%).</summary>
    public const decimal OverflowSafeYieldFraction = 0.19m;
}