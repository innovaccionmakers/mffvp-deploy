namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsProducts
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.products;

    public const string PolicyViewGoal = "fvp:products:goalsManagement:view";
    public const string PolicyUpdateGoal = "fvp:products:goalsManagement:update";
    public const string PolicyCreateGoal = "fvp:products:goalsManagement:create";

    // Objective
    public static readonly MakersPermission ViewGoal = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.goalsManagement, MakersActions.view,
        "Permite consultar objetivos.",
        "FVP", "Productos", "Objetivo", "Consultar"
    );

    public static readonly MakersPermission UpdateGoal = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.goalsManagement, MakersActions.update,
        "Permite modificar objetivos.",
        "FVP", "Productos", "Objetivo", "Modificar"
    );

    public static readonly MakersPermission CreateGoal = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.goalsManagement, MakersActions.create,
        "Permite crear objetivos.",
        "FVP", "Productos", "Objetivo", "Crear"
    );

    public static readonly List<MakersPermission> All = new()
    {
        ViewGoal,
        UpdateGoal,
        CreateGoal
    };
}
