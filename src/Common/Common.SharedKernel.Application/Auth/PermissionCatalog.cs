namespace Common.SharedKernel.Application.Auth;

public static class PermissionCatalog
{
    public static readonly Dictionary<string, string> Permissions = new()
    {
        ["Permission.MasterData:Insumo:Read"] = "Leer Insumos",
        ["Permission.MasterData:Insumo:Modify"] = "Modificar Insumos",
        ["Permission.MasterData:Core:Delete"] = "Eliminar Core",
        ["Permission.MasterData:Core:Modify"] = "Modificar Core"
    };
}