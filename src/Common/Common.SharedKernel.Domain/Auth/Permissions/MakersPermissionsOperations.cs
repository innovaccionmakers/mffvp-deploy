namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsOperations
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.operations;

    public const string PolicyViewIndividualOperations = "fvp:operations:passiveIndividualOperations:view";
    public const string PolicyExecuteIndividualOperations = "fvp:operations:passiveIndividualOperations:execute";

    public static readonly MakersPermission ViewIndividualOperations = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.passiveIndividualOperations, MakersActions.view,
        "Permite consultar operaciones individuales.",
        "FVP", "Operaciones Pasivas", "Operaciones Individuales", "Consultar"
    );

    public static readonly MakersPermission ExecuteIndividualOperations = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.passiveIndividualOperations, MakersActions.execute,
        "Permite ejecutar operaciones individuales.",
        "FVP", "Operaciones Pasivas", "Operaciones Individuales", "Ejecutar"
    );

    public static readonly List<MakersPermission> All = new()
    {
        ViewIndividualOperations,
        ExecuteIndividualOperations
    };
}

