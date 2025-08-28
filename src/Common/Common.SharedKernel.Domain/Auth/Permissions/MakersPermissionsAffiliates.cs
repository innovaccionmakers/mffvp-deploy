namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsAffiliates
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.affiliates;

    public const string PolicyViewAffiliateManagement = "fvp:affiliates:affiliatesManagement:view";
    public const string PolicyUpdateAffiliateManagement = "fvp:affiliates:affiliatesManagement:update";
    public const string PolicyActivateAffiliateManagement = "fvp:affiliates:affiliatesManagement:activate";
    public const string PolicyPensionRequirementsAffiliateManagement = "fvp:affiliates:affiliatesManagement:pensionRequirements";

    public const string PolicyViewGoal = "fvp:affiliates:goalsManagement:view";
    public const string PolicyUpdateGoal = "fvp:affiliates:goalsManagement:update";
    public const string PolicyCreateGoal = "fvp:affiliates:goalsManagement:create";

    // Affiliate Management
    public static readonly MakersPermission ViewAffiliateManagement = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7240-9a23-df12e4c7a501"),
        Module, Domain, MakersResources.affiliatesManagement, MakersActions.view,
        "Permite consultar administración de afiliados.",
        "FVP", "Afiliados", "Administración Afiliado", "Consultar"
    );

    public static readonly MakersPermission UpdateAffiliateManagement = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7241-8c45-b1e7fa36d502"),
        Module, Domain, MakersResources.affiliatesManagement, MakersActions.update,
        "Permite modificar administración de afiliados.",
        "FVP", "Afiliados", "Administración Afiliado", "Modificar"
    );

    public static readonly MakersPermission ActivateAffiliateManagement = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7242-9f56-c4b9d7e8f503"),
        Module, Domain, MakersResources.affiliatesManagement, MakersActions.activate,
        "Permite activar afiliados.",
        "FVP", "Afiliados", "Administración Afiliado", "Activar"
    );

    // Objective
    public static readonly MakersPermission ViewGoal = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7250-9c12-e3d9af47a601"),
        Module, Domain, MakersResources.goalsManagement, MakersActions.view,
        "Permite consultar objetivos.",
        "FVP", "Afiliados", "Objetivo", "Consultar"
    );

    public static readonly MakersPermission UpdateGoal = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7251-8d34-f2c8be58b602"),
        Module, Domain, MakersResources.goalsManagement, MakersActions.update,
        "Permite modificar objetivos.",
        "FVP", "Afiliados", "Objetivo", "Modificar"
    );

    public static readonly MakersPermission CreateGoal = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7252-9e45-a7b3dc69c603"),
        Module, Domain, MakersResources.goalsManagement, MakersActions.create,
        "Permite crear objetivos.",
        "FVP", "Afiliados", "Objetivo", "Crear"
    );

    public static readonly MakersPermission PensionRequirementsAffiliateManagement = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7253-8f78-a7b3dc69c604"),
        Module, Domain, MakersResources.affiliatesManagement, MakersActions.pensionRequirements,
        "Permite gestionar requisitos de pensión del afiliado.",
        "FVP", "Afiliados", "Administración Afiliado", "Requisitos Pensión"
    );

    public static readonly List<MakersPermission> All = new()
    {
        ViewAffiliateManagement,
        UpdateAffiliateManagement,
        ActivateAffiliateManagement,
        ViewGoal,
        UpdateGoal,
        CreateGoal,
        PensionRequirementsAffiliateManagement,
    };
}

