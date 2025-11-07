using Closing.Integrations.Common;

namespace Closing.Application.Closing.Services.Warnings;

/// <summary>
/// Catálogo en código de advertencias de negocio/técnicas **sin** persistencia en BD.
/// 
/// NOTA: Mientras no exista tabla o estructura oficial de almacenamiento, este catálogo
/// centraliza los códigos, textos y severidades que se publican vía `IWarningCollector`.
/// 
/// Convenciones de código:
///  - ADV*** : Advertencias generales (cobertura, faltantes, etc.)
///  - CTRL***: Controles/consistencias globales
///  - VAL*** : Validaciones específicas de cálculo/valoración/pyg
/// 
/// Severidad esperada: "Leve", "Media", "Crítica"
/// Prioridad: 1 = alta/urgente, 2 = media, 3 = baja
/// </summary>
public static class WarningCatalog
{
    public static WarningItem Adv001PygMissing() => new()
    {
        Code = "ADV001",
        Description = "Existen portafolios sin carga de PyG",
        Severity = "Leve",
        Priority = 2
    };


    public static WarningItem Adv003YieldDifference(decimal diff, decimal tolerance) => new()
    {
        Code = "ADV002",
        Description = $"Los rendimientos a abonar con relación a los abonados (Diferencia = {diff}) superan la tolerancia configurada (tolerancia = {tolerance})",
        Severity = "Media",
        Priority = 1
    };

    /// <summary>
    /// PRE-CIERRE: ingreso del día inusualmente alto vs **valoración del día anterior**.
    /// </summary>
    public static WarningItem Val004IncomeHighVsPreviousValuation(
        decimal dailyIncome,
        decimal previousValuationAmount,
        decimal appliedLimit)
        => new()
        {
            Code = "ADV003",
            Description =
                $"El ingreso del día ({dailyIncome}) es inusualmente alto respecto a la " +
                $"valoración del portafolio del día anterior ({previousValuationAmount}). " +
                $"Límite preliminar aplicado = {appliedLimit}. No se calculará la rentabilidad diaria.",
            Severity = "Crítica",
            Priority = 1
        };

    public static WarningItem Val005IncomeExceedsTodayValuation(
        decimal dailyIncome,
        decimal todayValuationAmount)
        => new()
        {
            Code = "ADV004",
            Description =
                $"El ingreso del día ({dailyIncome}) excede la valoración del portafolio del día ({todayValuationAmount}). " +
                "No se calculará la rentabilidad diaria para evitar resultados inválidos.",
            Severity = "Crítica",
            Priority = 1
        };

    /// <summary>
    /// Base de datos previa insuficiente para calcular: no hay valoración válida del día anterior.
    /// </summary>
    public static WarningItem Val006MissingPreviousValuation()
        => new()
        {
            Code = "ADV005",
            Description = "No existe una valoración válida del portafolio del día anterior. No se calculará la rentabilidad diaria.",
            Severity = "Leve",
            Priority = 2
        };

    public static WarningItem Adv006ExtraReturnMissing() => new()
    {
        Code = "ADV006",
        Description = "No se encontraron rendimientos extras para procesar",
        Severity = "Leve",
        Priority = 2
    };

    public static WarningItem Ctrl001Participation() => new()
    {
        Code = "CTRL001",
        Description = "La suma de participaciones no es 100% (desviación > tolerancia)",
        Severity = "Crítica",
        Priority = 1
    };
}
