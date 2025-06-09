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

public static class MakersPermissionsAssociateConfigurationParameters
{
    public static readonly string View = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.configurationParameters, MakersActions.view);
    public static readonly string Search = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.configurationParameters, MakersActions.search);
    public static readonly string Create = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.configurationParameters, MakersActions.create);
    public static readonly string Update = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.configurationParameters, MakersActions.update);
    public static readonly string Delete = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.associate, MakersResources.configurationParameters, MakersActions.delete);

    public static readonly Dictionary<string, string> All = new()
    {
        { View, "Permite ver parámetros de configuración (asociado)." },
        { Search, "Permite buscar parámetros de configuración (asociado)." },
        { Create, "Permite crear parámetros de configuración (asociado)." },
        { Update, "Permite actualizar parámetros de configuración (asociado)." },
        { Delete, "Permite eliminar parámetros de configuración (asociado)." }
    };
}

public static class MakersPermissionsOperationsAuxiliaryInformations
{
    public static readonly string View = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.auxiliaryInformations, MakersActions.view);
    public static readonly string Search = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.auxiliaryInformations, MakersActions.search);
    public static readonly string Create = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.auxiliaryInformations, MakersActions.create);
    public static readonly string Update = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.auxiliaryInformations, MakersActions.update);
    public static readonly string Delete = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.auxiliaryInformations, MakersActions.delete);

    public static readonly Dictionary<string, string> All = new()
    {
        { View, "Permite ver informaciones auxiliares (operaciones)." },
        { Search, "Permite buscar informaciones auxiliares (operaciones)." },
        { Create, "Permite crear informaciones auxiliares (operaciones)." },
        { Update, "Permite actualizar informaciones auxiliares (operaciones)." },
        { Delete, "Permite eliminar informaciones auxiliares (operaciones)." }
    };
}

public static class MakersPermissionsOperationsClientOperations
{
    public static readonly string View = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.clientOperations, MakersActions.view);
    public static readonly string Search = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.clientOperations, MakersActions.search);
    public static readonly string Create = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.clientOperations, MakersActions.create);
    public static readonly string Update = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.clientOperations, MakersActions.update);
    public static readonly string Delete = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.clientOperations, MakersActions.delete);

    public static readonly Dictionary<string, string> All = new()
    {
        { View, "Permite ver operaciones de cliente." },
        { Search, "Permite buscar operaciones de cliente." },
        { Create, "Permite crear operaciones de cliente." },
        { Update, "Permite actualizar operaciones de cliente." },
        { Delete, "Permite eliminar operaciones de cliente." }
    };
}

public static class MakersPermissionsOperationsConfigurationParameters
{
    public static readonly string View = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.configurationParameters, MakersActions.view);
    public static readonly string Search = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.configurationParameters, MakersActions.search);
    public static readonly string Create = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.configurationParameters, MakersActions.create);
    public static readonly string Update = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.configurationParameters, MakersActions.update);
    public static readonly string Delete = MakersPermission.NameFor(MakersModules.fvp, MakersDomains.operations, MakersResources.configurationParameters, MakersActions.delete);

    public static readonly Dictionary<string, string> All = new()
    {
        { View, "Permite ver parámetros de configuración (operaciones)." },
        { Search, "Permite buscar parámetros de configuración (operaciones)." },
        { Create, "Permite crear parámetros de configuración (operaciones)." },
        { Update, "Permite actualizar parámetros de configuración (operaciones)." },
        { Delete, "Permite eliminar parámetros de configuración (operaciones)." }
    };
}