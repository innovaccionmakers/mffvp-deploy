namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsOperations
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.operations;

    public const string PolicyExecuteIndividualOperations = "fvp:operations:passiveIndividualOperations:execute";
    public const string PolicyCancelOperations = "fvp:operations:voids:cancel";
    public const string PolicyCreateAccountingNote = "fvp:operations:accountingNotes:create";

    // Operaciones Individuales
    public static readonly MakersPermission ExecuteIndividualOperations = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7281-8c56-d9e4fba3c902"),
        Module, Domain, MakersResources.passiveIndividualOperations, MakersActions.execute,
        "Permite ejecutar operaciones individuales.",
        "FVP", "Operaciones Pasivas", "Operaciones Individuales", "Ejecutar"
    );

    // Anulaciones
    public static readonly MakersPermission CancelOperations = MakersPermissionFactory.Create(
        Guid.Parse("e3bf2886-03ff-439a-abc4-260f16cd2003"),
        Module, Domain, MakersResources.voids, MakersActions.cancel,
        "Permite anular operaciones.",
        "FVP", "Operaciones Pasivas", "Anulaciones", "Anular"
    );

    // Notas Contables
    public static readonly MakersPermission CreateAccountingNote = MakersPermissionFactory.Create(
        Guid.Parse("81588fb2-1553-4e65-aa16-4bcc4d4b51ea"),
        Module, Domain, MakersResources.accountingNotes, MakersActions.create,
        "Permite crear notas contables.",
        "FVP", "Operaciones Pasivas", "Notas Contables", "Crear"
    );

    public static readonly List<MakersPermission> All = new()
    {
        ExecuteIndividualOperations,
        CancelOperations,
        CreateAccountingNote
    };
}

