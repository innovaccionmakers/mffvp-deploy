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










