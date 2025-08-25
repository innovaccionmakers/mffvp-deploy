using Closing.Integrations.Common;

namespace Closing.Application.Closing.Services.Warnings;
public static class WarningCatalog
{
    public static WarningItem Adv001PygMissing() => new()
    {
        Code = "ADV001",
        Description = "Existen portafolios sin carga de PyG",
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

    public static WarningItem Val003YieldDifference(decimal diff, decimal tolerance) => new()
    {
        Code = "VAL003",
        Description = $"Los rendimientos a abonar con relación a los abonados (Diferencia = {diff}) superan la tolerancia configurada (tolerancia = {tolerance})",
        Severity = "Media",
        Priority = 1
    };
}