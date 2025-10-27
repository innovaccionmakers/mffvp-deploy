namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsOperations
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.operations;

    public const string PolicyExecuteIndividualOperations = "fvp:operations:passiveIndividualOperations:execute";
    public const string PolicyCreateDebitNote = "fvp:operations:passiveIndividualOperations:create";
    public const string PolicyCancelIndividualOperations = "fvp:operations:passiveIndividualOperations:cancel";

    public static readonly MakersPermission ExecuteIndividualOperations = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7281-8c56-d9e4fba3c902"),
        Module, Domain, MakersResources.passiveIndividualOperations, MakersActions.execute,
        "Permite ejecutar operaciones individuales.",
        "FVP", "Operaciones Pasivas", "Operaciones Individuales", "Ejecutar"
    );

    public static readonly MakersPermission CreateDebitNote = MakersPermissionFactory.Create(
        Guid.Parse("81588fb2-1553-4e65-aa16-4bcc4d4b51ea"),
        Module, Domain, MakersResources.passiveIndividualOperations, MakersActions.create,
        "Permite crear notas débito individuales.",
        "FVP", "Operaciones Pasivas", "Notas Débito", "Crear"
    );

    public static readonly MakersPermission CancelIndividualOperations = MakersPermissionFactory.Create(
        Guid.Parse("e3bf2886-03ff-439a-abc4-260f16cd2003"),
        Module, Domain, MakersResources.passiveIndividualOperations, MakersActions.cancel,
        "Permite anular operaciones individuales.",
        "FVP", "Operaciones Pasivas", "Anulaciones", "Anular"
    );

    public static readonly List<MakersPermission> All = new()
    {
        ExecuteIndividualOperations,
        CreateDebitNote,
        CancelIndividualOperations
    };
}

