namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsUsers
{
    public static readonly string View = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.users, MakersResources.users, MakersActions.view);
    public static readonly string Search = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.users, MakersResources.users, MakersActions.search);
    public static readonly string Create = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.users, MakersResources.users, MakersActions.create);
    public static readonly string Update = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.users, MakersResources.users, MakersActions.update);
    public static readonly string Delete = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.users, MakersResources.users, MakersActions.delete);

    public static readonly Dictionary<string, string> All = new()
    {
        { View, "Permite ver los usuarios." },
        { Search, "Permite buscar usuarios." },
        { Create, "Permite crear nuevos usuarios." },
        { Update, "Permite editar usuarios." },
        { Delete, "Permite eliminar usuarios." }
    };
}

public static class MakersPermissionsAssociateActivates
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.associate;
    private const string Resource = MakersResources.activates;

    public static readonly MakersPermission View = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.view,
        "Permite ver activaciones de asociados.",
        "FVP", "Asociados", "Activaciones", "Ver"
    );

    public static readonly MakersPermission Create = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.create,
        "Permite crear activaciones de asociados.",
        "FVP", "Asociados", "Activaciones", "Crear"
    );

    public static readonly MakersPermission Update = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.update,
        "Permite actualizar activaciones de asociados.",
        "FVP", "Asociados", "Activaciones", "Actualizar"
    );

    public static readonly List<MakersPermission> All = new()
    {
        View, Create, Update
    };
}

public static class MakersPermissionsAssociatePensionRequirements
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.associate;
    private const string Resource = MakersResources.pensionRequirements;

    public static readonly MakersPermission View = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.view,
        "Permite ver requisitos de pensión.",
        "FVP", "Asociados", "Requisitos de Pensión", "Ver"
    );

    public static readonly MakersPermission Create = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.create,
        "Permite crear requisitos de pensión.",
        "FVP", "Asociados", "Requisitos de Pensión", "Crear"
    );

    public static readonly MakersPermission Update = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.update,
        "Permite actualizar requisitos de pensión.",
        "FVP", "Asociados", "Requisitos de Pensión", "Actualizar"
    );

    public static readonly List<MakersPermission> All = new()
    {
        View, Create, Update
    };
}

public static class MakersPermissionsOperationsContributionTx
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.operations;
    private const string Resource = MakersResources.contributiontx;

    public static readonly MakersPermission Create = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.create,
        "Permite registrar un aporte.",
        "FVP", "Operaciones", "ContributionTx", "Crear"
    );

    public static readonly List<MakersPermission> All = new()
    {
        Create
    };
}
