namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsClosing
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.closing;

    // Policy constants
    public const string PolicyViewLoadProfitAndLost = "fvp:closing:closingLoadProfitAndLost:view";
    public const string PolicyCreateLoadProfitAndLost = "fvp:closing:closingLoadProfitAndLost:create";

    public const string PolicyViewSimulation = "fvp:closing:closingSimulation:view";
    public const string PolicyExecuteSimulation = "fvp:closing:closingSimulation:execute";

    public const string PolicyViewClosure = "fvp:closing:closingExecution:view";
    public const string PolicyExecuteClosure = "fvp:closing:closingExecution:execute";

    public static readonly MakersPermission ViewLoadProfitAndLost = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.closingLoadProfitAndLost, MakersActions.view,
        "Permite consultar la carga de PyG.",
        "FVP", "Cierre", "Carga PyG", "Consultar"
    );

    public static readonly MakersPermission CreateLoadProfitAndLost = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.closingLoadProfitAndLost, MakersActions.create,
        "Permite crear registros en la carga de PyG.",
        "FVP", "Cierre", "Carga PyG", "Crear"
    );

    public static readonly MakersPermission ViewSimulation = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.closingSimulation, MakersActions.view,
        "Permite consultar la simulación de cierre.",
        "FVP", "Cierre", "Simulación Cierre", "Consultar"
    );

    public static readonly MakersPermission ExecuteSimulation = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.closingSimulation, MakersActions.execute,
        "Permite ejecutar la simulación de cierre.",
        "FVP", "Cierre", "Simulación Cierre", "Ejecutar"
    );

    public static readonly MakersPermission ViewClosure = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.closingExecution, MakersActions.view,
        "Permite consultar el cierre.",
        "FVP", "Cierre", "Cierre", "Consultar"
    );

    public static readonly MakersPermission ExecuteClosure = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.closingExecution, MakersActions.execute,
        "Permite ejecutar el cierre.",
        "FVP", "Cierre", "Cierre", "Ejecutar"
    );

    public static readonly List<MakersPermission> All = new()
    {
        ViewLoadProfitAndLost, CreateLoadProfitAndLost,
        ViewSimulation, ExecuteSimulation,
        ViewClosure, ExecuteClosure
    };
}
