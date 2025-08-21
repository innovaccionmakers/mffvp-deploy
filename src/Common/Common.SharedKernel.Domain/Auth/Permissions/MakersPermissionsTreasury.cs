namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsTreasury
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.treasury;

    // Conceptos Ingreso/Gastos
    public static readonly MakersPermission ViewConcepts = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-72a0-9f67-d3a4c7e1ac01"),
        Module, Domain, MakersResources.treasuryConcepts, MakersActions.view,
        "Permite consultar conceptos de ingresos y gastos.",
        "FVP", "Tesorería", "Conceptos Ingreso/Gastos", "Consultar"
    );

    public static readonly MakersPermission UpdateConcepts = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-72a1-8a78-e4b6dce2ad02"),
        Module, Domain, MakersResources.treasuryConcepts, MakersActions.update,
        "Permite modificar conceptos de ingresos y gastos.",
        "FVP", "Tesorería", "Conceptos Ingreso/Gastos", "Modificar"
    );

    public static readonly MakersPermission CreateConcepts = MakersPermissionFactory.Create(
         Guid.Parse("018f1e2c-9e47-72a2-9b89-f5c7efd3ae03"),
        Module, Domain, MakersResources.treasuryConcepts, MakersActions.create,
        "Permite crear conceptos de ingresos y gastos.",
        "FVP", "Tesorería", "Conceptos Ingreso/Gastos", "Crear"
    );

    // Cargar Ingresos/Gastos Individuales
    public static readonly MakersPermission ViewIndividualRecords = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-72b0-8c90-a6d8efe3af01"),
        Module, Domain, MakersResources.treasuryLoadIndividualRecords, MakersActions.view,
        "Permite consultar ingresos y gastos individuales.",
        "FVP", "Tesorería", "Cargar Ingresos/Gastos Individuales", "Consultar"
    );

    public static readonly MakersPermission CreateIndividualRecords = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-72b1-9da1-b7e9f0f4b002"),
        Module, Domain, MakersResources.treasuryLoadIndividualRecords, MakersActions.create,
        "Permite crear ingresos y gastos individuales.",
        "FVP", "Tesorería", "Cargar Ingresos/Gastos Individuales", "Crear"
    );

    public static readonly MakersPermission DeleteIndividualRecords = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-72b2-8eb2-c8f0a105c103"),
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
