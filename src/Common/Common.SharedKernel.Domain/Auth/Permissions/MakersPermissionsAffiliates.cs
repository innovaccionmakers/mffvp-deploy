namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsAffiliates
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.affiliates;

    public const string PolicyViewAffiliateManagement = "fvp:affiliates:affiliatesManagement:view";
    public const string PolicyUpdateAffiliateManagement = "fvp:affiliates:affiliatesManagement:update";
    public const string PolicyActivateAffiliateManagement = "fvp:affiliates:affiliatesManagement:activate";
    public const string PolicyPensionRequirementsAffiliateManagement = "fvp:affiliates:affiliatesManagement:pensionRequirements";

    public const string PolicyViewPensionRequirements = "fvp:affiliates:pensionRequirements:view";
    public const string PolicyUpdatePensionRequirements = "fvp:affiliates:pensionRequirements:update";
    public const string PolicyCreatePensionRequirements = "fvp:affiliates:pensionRequirements:create";

    // Affiliate Management
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

    // Pension Requirements
    public static readonly MakersPermission ViewPensionRequirements = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.pensionRequirements, MakersActions.view,
        "Permite consultar requisitos de pensión del afiliado.",
        "FVP", "Afiliados", "Requisitos de Pensión", "Consultar"
    );

    public static readonly MakersPermission UpdatePensionRequirements = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.pensionRequirements, MakersActions.update,
        "Permite modificar requisitos de pensión del afiliado.",
        "FVP", "Afiliados", "Requisitos de Pensión", "Modificar"
    );

    public static readonly MakersPermission CreatePensionRequirements = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.pensionRequirements, MakersActions.create,
        "Permite crear requisitos de pensión del afiliado.",
        "FVP", "Afiliados", "Requisitos de Pensión", "Crear"
    );

    public static readonly List<MakersPermission> All = new()
    {
        ViewAffiliateManagement,
        UpdateAffiliateManagement,
        ActivateAffiliateManagement,
        PensionRequirementsAffiliateManagement,
        ViewPensionRequirements,
        UpdatePensionRequirements,
        CreatePensionRequirements
    };
}

