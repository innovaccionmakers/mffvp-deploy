namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsAffiliates
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.affiliates;

    // Administración Afiliado
    public static readonly MakersPermission ViewAffiliateManagement = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.affiliatesManagement, MakersActions.view,
        "Permite consultar administración de afiliados.",
        "FVP", "Afiliados", "Administración Afiliado", "Consultar"
    );

    public static readonly MakersPermission UpdateAffiliateManagement = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.affiliatesManagement, MakersActions.update,
        "Permite modificar administración de afiliados.",
        "FVP", "Afiliados", "Administración Afiliado", "Modificar"
    );

    public static readonly MakersPermission ActivateAffiliateManagement = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.affiliatesManagement, MakersActions.activate,
        "Permite activar afiliados.",
        "FVP", "Afiliados", "Administración Afiliado", "Activar"
    );

    public static readonly MakersPermission PensionRequirementsAffiliateManagement = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.affiliatesManagement, MakersActions.pensionRequirements,
        "Permite gestionar requisitos de pensión del afiliado.",
        "FVP", "Afiliados", "Administración Afiliado", "Requisitos Pensión"
    );

    // Objetivo
    public static readonly MakersPermission ViewAffiliateObjective = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.affiliatesObjective, MakersActions.view,
        "Permite consultar objetivos del afiliado.",
        "FVP", "Afiliados", "Objetivo", "Consultar"
    );

    public static readonly MakersPermission UpdateAffiliateObjective = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.affiliatesObjective, MakersActions.update,
        "Permite modificar objetivos del afiliado.",
        "FVP", "Afiliados", "Objetivo", "Modificar"
    );

    public static readonly MakersPermission CreateAffiliateObjective = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.affiliatesObjective, MakersActions.create,
        "Permite crear objetivos del afiliado.",
        "FVP", "Afiliados", "Objetivo", "Crear"
    );

    public static readonly List<MakersPermission> All = new()
    {
        ViewAffiliateManagement,
        UpdateAffiliateManagement,
        ActivateAffiliateManagement,
        PensionRequirementsAffiliateManagement,
        ViewAffiliateObjective,
        UpdateAffiliateObjective,
        CreateAffiliateObjective
    };
}

