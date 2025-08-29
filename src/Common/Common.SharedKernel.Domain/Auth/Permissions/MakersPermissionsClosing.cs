namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsClosing
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.closing;

    // Policy constants
    public const string PolicyViewLoadProfitAndLost = "fvp:closing:closingLoadProfitAndLost:view";
    public const string PolicyCreateLoadProfitAndLost = "fvp:closing:closingLoadProfitAndLost:create";

    public const string PolicyExecuteSimulation = "fvp:closing:closingSimulation:execute";

    public const string PolicyExecuteClosure = "fvp:closing:closingExecution:execute";

    public static readonly MakersPermission ViewLoadProfitAndLost = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7260-9b23-d1f3a7c6a701"),
        Module, Domain, MakersResources.closingLoadProfitAndLost, MakersActions.view,
        "Permite consultar la carga de PyG.",
        "FVP", "Cierre", "Carga PyG", "Consultar"
    );

    public static readonly MakersPermission CreateLoadProfitAndLost = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7261-8c34-b9d5e8f4c702"),
        Module, Domain, MakersResources.closingLoadProfitAndLost, MakersActions.create,
        "Permite crear registros en la carga de PyG.",
        "FVP", "Cierre", "Carga PyG", "Crear"
    );

    public static readonly MakersPermission ExecuteSimulation = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7263-8d56-f8c9ba47e704"),
        Module, Domain, MakersResources.closingSimulation, MakersActions.execute,
        "Permite ejecutar la simulación de cierre.",
        "FVP", "Cierre", "Simulación Cierre", "Ejecutar"
    );

    public static readonly MakersPermission ExecuteClosure = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7265-8b78-f1d9ca6bd706"),
        Module, Domain, MakersResources.closingExecution, MakersActions.execute,
        "Permite ejecutar el cierre.",
        "FVP", "Cierre", "Cierre", "Ejecutar"
    );

    public static readonly List<MakersPermission> All = new()
    {
        ViewLoadProfitAndLost, CreateLoadProfitAndLost,
        ExecuteSimulation,
        ExecuteClosure
    };
}
