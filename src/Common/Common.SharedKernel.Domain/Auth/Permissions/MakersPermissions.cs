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
    public static readonly string View = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.activates, MakersActions.view);
    public static readonly string Search = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.activates, MakersActions.search);
    public static readonly string Create = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.activates, MakersActions.create);
    public static readonly string Update = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.activates, MakersActions.update);
    public static readonly string Delete = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.activates, MakersActions.delete);

    public static readonly Dictionary<string, string> All = new()
    {
        { View, "Permite ver activaciones de asociados." },
        { Search, "Permite buscar activaciones de asociados." },
        { Create, "Permite crear activaciones de asociados." },
        { Update, "Permite actualizar activaciones de asociados." },
        { Delete, "Permite eliminar activaciones de asociados." }
    };
}

public static class MakersPermissionsAssociatePensionRequirements
{
    public static readonly string View = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.pensionRequirements, MakersActions.view);
    public static readonly string Search = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.pensionRequirements, MakersActions.search);
    public static readonly string Create = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.pensionRequirements, MakersActions.create);
    public static readonly string Update = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.pensionRequirements, MakersActions.update);
    public static readonly string Delete = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.pensionRequirements, MakersActions.delete);

    public static readonly Dictionary<string, string> All = new()
    {
        { View, "Permite ver requisitos de pensión." },
        { Search, "Permite buscar requisitos de pensión." },
        { Create, "Permite crear requisitos de pensión." },
        { Update, "Permite actualizar requisitos de pensión." },
        { Delete, "Permite eliminar requisitos de pensión." }
    };
}



public static class MakersPermissionsOperationsAuxiliaryInformations
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.operations;
    private const string Resource = MakersResources.auxiliaryInformations;

    public static readonly MakersPermission View = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.view,
        "Permite ver informaciones auxiliares (operaciones).",
        "FVP", "Operaciones", "Información Auxiliar", "Ver"
    );

    public static readonly MakersPermission Search = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.search,
        "Permite buscar informaciones auxiliares (operaciones).",
        "FVP", "Operaciones", "Información Auxiliar", "Buscar"
    );

    public static readonly MakersPermission Create = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.create,
        "Permite crear informaciones auxiliares (operaciones).",
        "FVP", "Operaciones", "Información Auxiliar", "Crear"
    );

    public static readonly MakersPermission Update = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.update,
        "Permite actualizar informaciones auxiliares (operaciones).",
        "FVP", "Operaciones", "Información Auxiliar", "Actualizar"
    );

    public static readonly MakersPermission Delete = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.delete,
        "Permite eliminar informaciones auxiliares (operaciones).",
        "FVP", "Operaciones", "Información Auxiliar", "Eliminar"
    );

    public static readonly List<MakersPermission> All = new()
    {
        View, Search, Create, Update, Delete
    };
}


public static class MakersPermissionsOperationsClientOperations
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.operations;
    private const string Resource = MakersResources.clientOperations;

    public static readonly MakersPermission View = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.view,
        "Permite ver operaciones de cliente.",
        "FVP", "Operaciones", "Operaciones de Cliente", "Ver"
    );

    public static readonly MakersPermission Search = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.search,
        "Permite buscar operaciones de cliente.",
        "FVP", "Operaciones", "Operaciones de Cliente", "Buscar"
    );

    public static readonly MakersPermission Create = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.create,
        "Permite crear operaciones de cliente.",
        "FVP", "Operaciones", "Operaciones de Cliente", "Crear"
    );

    public static readonly MakersPermission Update = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.update,
        "Permite actualizar operaciones de cliente.",
        "FVP", "Operaciones", "Operaciones de Cliente", "Actualizar"
    );

    public static readonly MakersPermission Delete = MakersPermissionFactory.Create(
        Module, Domain, Resource, MakersActions.delete,
        "Permite eliminar operaciones de cliente.",
        "FVP", "Operaciones", "Operaciones de Cliente", "Eliminar"
    );

    public static readonly List<MakersPermission> All = new()
    {
        View, Search, Create, Update, Delete
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
