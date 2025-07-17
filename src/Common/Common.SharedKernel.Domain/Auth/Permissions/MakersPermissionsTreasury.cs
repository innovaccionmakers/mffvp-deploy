namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsTreasury
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.treasury;

    // Conceptos Ingreso/Gastos
    public static readonly MakersPermission ViewConcepts = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.treasuryConcepts, MakersActions.view,
        "Permite consultar conceptos de ingresos y gastos.",
        "FVP", "Tesorería", "Conceptos Ingreso/Gastos", "Consultar"
    );

    public static readonly MakersPermission UpdateConcepts = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.treasuryConcepts, MakersActions.update,
        "Permite modificar conceptos de ingresos y gastos.",
        "FVP", "Tesorería", "Conceptos Ingreso/Gastos", "Modificar"
    );

    public static readonly MakersPermission CreateConcepts = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.treasuryConcepts, MakersActions.create,
        "Permite crear conceptos de ingresos y gastos.",
        "FVP", "Tesorería", "Conceptos Ingreso/Gastos", "Crear"
    );

    // Cargar Ingresos/Gastos Individuales
    public static readonly MakersPermission ViewIndividualRecords = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.treasuryLoadIndividualRecords, MakersActions.view,
        "Permite consultar ingresos y gastos individuales.",
        "FVP", "Tesorería", "Cargar Ingresos/Gastos Individuales", "Consultar"
    );

    public static readonly MakersPermission CreateIndividualRecords = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.treasuryLoadIndividualRecords, MakersActions.create,
        "Permite crear ingresos y gastos individuales.",
        "FVP", "Tesorería", "Cargar Ingresos/Gastos Individuales", "Crear"
    );

    public static readonly MakersPermission DeleteIndividualRecords = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.treasuryLoadIndividualRecords, MakersActions.delete,
        "Permite eliminar ingresos y gastos individuales.",
        "FVP", "Tesorería", "Cargar Ingresos/Gastos Individuales", "Eliminar"
    );

    public static readonly List<MakersPermission> All = new()
    {
        ViewConcepts, UpdateConcepts, CreateConcepts,
        ViewIndividualRecords, CreateIndividualRecords, DeleteIndividualRecords
    };
}
