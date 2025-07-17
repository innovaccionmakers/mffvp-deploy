namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsAccounting
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.accounting;

    // Configuración Cuentas Contables
    public static readonly MakersPermission CreateAccountSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingAccountSettings, MakersActions.create,
        "Permite crear cuentas contables.",
        "FVP", "Contabilidad", "Configuración Cuentas Contables", "Crear"
    );

    public static readonly MakersPermission UpdateAccountSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingAccountSettings, MakersActions.update,
        "Permite modificar cuentas contables.",
        "FVP", "Contabilidad", "Configuración Cuentas Contables", "Modificar"
    );

    public static readonly MakersPermission DeleteAccountSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingAccountSettings, MakersActions.delete,
        "Permite eliminar cuentas contables.",
        "FVP", "Contabilidad", "Configuración Cuentas Contables", "Eliminar"
    );

    public static readonly MakersPermission ViewAccountSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingAccountSettings, MakersActions.view,
        "Permite consultar cuentas contables.",
        "FVP", "Contabilidad", "Configuración Cuentas Contables", "Consultar"
    );

    // Configuración General
    public static readonly MakersPermission CreateGeneralSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingGeneralSettings, MakersActions.create,
        "Permite crear configuración general.",
        "FVP", "Contabilidad", "Configuración General", "Crear"
    );

    public static readonly MakersPermission UpdateGeneralSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingGeneralSettings, MakersActions.update,
        "Permite modificar configuración general.",
        "FVP", "Contabilidad", "Configuración General", "Modificar"
    );

    public static readonly MakersPermission DeleteGeneralSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingGeneralSettings, MakersActions.delete,
        "Permite eliminar configuración general.",
        "FVP", "Contabilidad", "Configuración General", "Eliminar"
    );

    public static readonly MakersPermission ViewGeneralSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingGeneralSettings, MakersActions.view,
        "Permite consultar configuración general.",
        "FVP", "Contabilidad", "Configuración General", "Consultar"
    );

    // Configuración Operaciones
    public static readonly MakersPermission CreateOperationSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingOperationSettings, MakersActions.create,
        "Permite crear configuración de operaciones.",
        "FVP", "Contabilidad", "Configuración Operaciones", "Crear"
    );

    public static readonly MakersPermission UpdateOperationSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingOperationSettings, MakersActions.update,
        "Permite modificar configuración de operaciones.",
        "FVP", "Contabilidad", "Configuración Operaciones", "Modificar"
    );

    public static readonly MakersPermission DeleteOperationSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingOperationSettings, MakersActions.delete,
        "Permite eliminar configuración de operaciones.",
        "FVP", "Contabilidad", "Configuración Operaciones", "Eliminar"
    );

    public static readonly MakersPermission ViewOperationSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingOperationSettings, MakersActions.view,
        "Permite consultar configuración de operaciones.",
        "FVP", "Contabilidad", "Configuración Operaciones", "Consultar"
    );

    // Configuración Tesorería
    public static readonly MakersPermission CreateTreasurySettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingTreasurySettings, MakersActions.create,
        "Permite crear configuración de tesorería.",
        "FVP", "Contabilidad", "Configuración Tesorería", "Crear"
    );

    public static readonly MakersPermission UpdateTreasurySettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingTreasurySettings, MakersActions.update,
        "Permite modificar configuración de tesorería.",
        "FVP", "Contabilidad", "Configuración Tesorería", "Modificar"
    );

    public static readonly MakersPermission DeleteTreasurySettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingTreasurySettings, MakersActions.delete,
        "Permite eliminar configuración de tesorería.",
        "FVP", "Contabilidad", "Configuración Tesorería", "Eliminar"
    );

    public static readonly MakersPermission ViewTreasurySettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingTreasurySettings, MakersActions.view,
        "Permite consultar configuración de tesorería.",
        "FVP", "Contabilidad", "Configuración Tesorería", "Consultar"
    );

    // Configuración Conceptos
    public static readonly MakersPermission CreateConceptSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingConceptSettings, MakersActions.create,
        "Permite crear configuración de conceptos.",
        "FVP", "Contabilidad", "Configuración Conceptos", "Crear"
    );

    public static readonly MakersPermission UpdateConceptSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingConceptSettings, MakersActions.update,
        "Permite modificar configuración de conceptos.",
        "FVP", "Contabilidad", "Configuración Conceptos", "Modificar"
    );

    public static readonly MakersPermission DeleteConceptSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingConceptSettings, MakersActions.delete,
        "Permite eliminar configuración de conceptos.",
        "FVP", "Contabilidad", "Configuración Conceptos", "Eliminar"
    );

    public static readonly MakersPermission ViewConceptSettings = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingConceptSettings, MakersActions.view,
        "Permite consultar configuración de conceptos.",
        "FVP", "Contabilidad", "Configuración Conceptos", "Consultar"
    );

    // Generación Contabilidad
    public static readonly MakersPermission ProcessGeneration = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingGeneration, MakersActions.process,
        "Permite procesar generación contable.",
        "FVP", "Contabilidad", "Generación Contabilidad", "Procesar"
    );

    public static readonly MakersPermission GenerateGeneration = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingGeneration, MakersActions.generate,
        "Permite generar registros contables.",
        "FVP", "Contabilidad", "Generación Contabilidad", "Generar"
    );

    public static readonly MakersPermission ValidateGeneration = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingGeneration, MakersActions.validate,
        "Permite validar registros contables.",
        "FVP", "Contabilidad", "Generación Contabilidad", "Validar"
    );

    public static readonly MakersPermission DeleteGeneration = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingGeneration, MakersActions.delete,
        "Permite eliminar generación contable.",
        "FVP", "Contabilidad", "Generación Contabilidad", "Eliminar"
    );

    public static readonly MakersPermission ViewGeneration = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.accountingGeneration, MakersActions.view,
        "Permite consultar generación contable.",
        "FVP", "Contabilidad", "Generación Contabilidad", "Consultar"
    );

    public static readonly List<MakersPermission> All = new()
    {
        CreateAccountSettings, UpdateAccountSettings, DeleteAccountSettings, ViewAccountSettings,
        CreateGeneralSettings, UpdateGeneralSettings, DeleteGeneralSettings, ViewGeneralSettings,
        CreateOperationSettings, UpdateOperationSettings, DeleteOperationSettings, ViewOperationSettings,
        CreateTreasurySettings, UpdateTreasurySettings, DeleteTreasurySettings, ViewTreasurySettings,
        CreateConceptSettings, UpdateConceptSettings, DeleteConceptSettings, ViewConceptSettings,
        ProcessGeneration, GenerateGeneration, ValidateGeneration, DeleteGeneration, ViewGeneration
    };
}

