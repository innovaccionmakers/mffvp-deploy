namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsOperations
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.operations;

    public const string PolicyExecuteIndividualOperations = "fvp:operations:passiveIndividualOperations:execute";

    public static readonly MakersPermission ExecuteIndividualOperations = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7281-8c56-d9e4fba3c902"),
        Module, Domain, MakersResources.passiveIndividualOperations, MakersActions.execute,
        "Permite ejecutar operaciones individuales.",
        "FVP", "Operaciones Pasivas", "Operaciones Individuales", "Ejecutar"
    );

    public static readonly List<MakersPermission> All = new()
    {
        ExecuteIndividualOperations
    };
}

