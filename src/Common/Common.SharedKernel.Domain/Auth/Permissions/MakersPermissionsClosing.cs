namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsClosing
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.closing;

    public static readonly MakersPermission ViewLoadPnL = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.closingLoadPnL, MakersActions.view,
        "Permite consultar la carga de PyG.",
        "FVP", "Cierre", "Carga PyG", "Consultar"
    );

    public static readonly MakersPermission CreateLoadPnL = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.closingLoadPnL, MakersActions.create,
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
        ViewLoadPnL, CreateLoadPnL,
        ViewSimulation, ExecuteSimulation,
        ViewClosure, ExecuteClosure
    };
}
