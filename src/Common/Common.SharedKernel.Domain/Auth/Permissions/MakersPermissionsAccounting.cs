namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsAccounting
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.accounting;

    // Configuración Cuentas Contables
    public static readonly MakersPermission CreateAccountSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-71e0-bd63-b8439e7a8f10"),
        Module, Domain, MakersResources.accountingAccountSettings, MakersActions.create,
        "Permite crear cuentas contables.",
        "FVP", "Contabilidad", "Configuración Cuentas Contables", "Crear"
    );

    public static readonly MakersPermission UpdateAccountSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-71e1-8a35-03b5f85a5c22"),
        Module, Domain, MakersResources.accountingAccountSettings, MakersActions.update,
        "Permite modificar cuentas contables.",
        "FVP", "Contabilidad", "Configuración Cuentas Contables", "Modificar"
    );

    public static readonly MakersPermission DeleteAccountSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-71e2-9d1b-7e6e3dbeac44"),
        Module, Domain, MakersResources.accountingAccountSettings, MakersActions.delete,
        "Permite eliminar cuentas contables.",
        "FVP", "Contabilidad", "Configuración Cuentas Contables", "Eliminar"
    );

    public static readonly MakersPermission ViewAccountSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-71e3-8c5b-f23dbf14d566"),
        Module, Domain, MakersResources.accountingAccountSettings, MakersActions.view,
        "Permite consultar cuentas contables.",
        "FVP", "Contabilidad", "Configuración Cuentas Contables", "Consultar"
    );

    // Configuración General
    public static readonly MakersPermission CreateGeneralSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-71f0-9f21-82b65ef1b001"),
        Module, Domain, MakersResources.accountingGeneralSettings, MakersActions.create,
        "Permite crear configuración general.",
        "FVP", "Contabilidad", "Configuración General", "Crear"
    );

    public static readonly MakersPermission UpdateGeneralSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-71f1-8b02-12a3d8f2b002"),
        Module, Domain, MakersResources.accountingGeneralSettings, MakersActions.update,
        "Permite modificar configuración general.",
        "FVP", "Contabilidad", "Configuración General", "Modificar"
    );

    public static readonly MakersPermission DeleteGeneralSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-71f2-b512-ff43c7a4c003"),
        Module, Domain, MakersResources.accountingGeneralSettings, MakersActions.delete,
        "Permite eliminar configuración general.",
        "FVP", "Contabilidad", "Configuración General", "Eliminar"
    );

    public static readonly MakersPermission ViewGeneralSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-71f3-82f1-cc983ab8d004"),
        Module, Domain, MakersResources.accountingGeneralSettings, MakersActions.view,
        "Permite consultar configuración general.",
        "FVP", "Contabilidad", "Configuración General", "Consultar"
    );

    // Configuración Operaciones
    public static readonly MakersPermission CreateOperationSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7200-9c22-b5f84ab1a101"),
        Module, Domain, MakersResources.accountingOperationSettings, MakersActions.create,
        "Permite crear configuración de operaciones.",
        "FVP", "Contabilidad", "Configuración Operaciones", "Crear"
    );

    public static readonly MakersPermission UpdateOperationSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7201-8f43-93c75de6b102"),
        Module, Domain, MakersResources.accountingOperationSettings, MakersActions.update,
        "Permite modificar configuración de operaciones.",
        "FVP", "Contabilidad", "Configuración Operaciones", "Modificar"
    );

    public static readonly MakersPermission DeleteOperationSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7202-9d12-ef67cfa8c103"),
        Module, Domain, MakersResources.accountingOperationSettings, MakersActions.delete,
        "Permite eliminar configuración de operaciones.",
        "FVP", "Contabilidad", "Configuración Operaciones", "Eliminar"
    );

    public static readonly MakersPermission ViewOperationSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7203-8e32-cfe29db3f104"),
        Module, Domain, MakersResources.accountingOperationSettings, MakersActions.view,
        "Permite consultar configuración de operaciones.",
        "FVP", "Contabilidad", "Configuración Operaciones", "Consultar"
    );

    // Configuración Tesorería
    public static readonly MakersPermission CreateTreasurySettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7210-9f12-d2a4e7c5a201"),
        Module, Domain, MakersResources.accountingTreasurySettings, MakersActions.create,
        "Permite crear configuración de tesorería.",
        "FVP", "Contabilidad", "Configuración Tesorería", "Crear"
    );

    public static readonly MakersPermission UpdateTreasurySettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7211-8c34-f1b7a8d9b202"),
        Module, Domain, MakersResources.accountingTreasurySettings, MakersActions.update,
        "Permite modificar configuración de tesorería.",
        "FVP", "Contabilidad", "Configuración Tesorería", "Modificar"
    );

    public static readonly MakersPermission DeleteTreasurySettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7212-9e45-cc74ab83c203"),
        Module, Domain, MakersResources.accountingTreasurySettings, MakersActions.delete,
        "Permite eliminar configuración de tesorería.",
        "FVP", "Contabilidad", "Configuración Tesorería", "Eliminar"
    );

    public static readonly MakersPermission ViewTreasurySettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7213-8b67-a47b9c52d204"),
        Module, Domain, MakersResources.accountingTreasurySettings, MakersActions.view,
        "Permite consultar configuración de tesorería.",
        "FVP", "Contabilidad", "Configuración Tesorería", "Consultar"
    );

    // Configuración Conceptos
    public static readonly MakersPermission CreateConceptSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7220-9f12-e1c8a4b5d301"),
        Module, Domain, MakersResources.accountingConceptSettings, MakersActions.create,
        "Permite crear configuración de conceptos.",
        "FVP", "Contabilidad", "Configuración Conceptos", "Crear"
    );

    public static readonly MakersPermission UpdateConceptSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7221-8d23-b7e9cf42a302"),
        Module, Domain, MakersResources.accountingConceptSettings, MakersActions.update,
        "Permite modificar configuración de conceptos.",
        "FVP", "Contabilidad", "Configuración Conceptos", "Modificar"
    );

    public static readonly MakersPermission DeleteConceptSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7222-9c89-cf54e7a1f303"),
        Module, Domain, MakersResources.accountingConceptSettings, MakersActions.delete,
        "Permite eliminar configuración de conceptos.",
        "FVP", "Contabilidad", "Configuración Conceptos", "Eliminar"
    );

    public static readonly MakersPermission ViewConceptSettings = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7223-8f34-9b76e4d9a304"),
        Module, Domain, MakersResources.accountingConceptSettings, MakersActions.view,
        "Permite consultar configuración de conceptos.",
        "FVP", "Contabilidad", "Configuración Conceptos", "Consultar"
    );

    // Generación Contabilidad
    public static readonly MakersPermission ProcessGeneration = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7230-9e12-bc47a9f6d401"),
        Module, Domain, MakersResources.accountingGeneration, MakersActions.process,
        "Permite procesar generación contable.",
        "FVP", "Contabilidad", "Generación Contabilidad", "Procesar"
    );

    public static readonly MakersPermission GenerateGeneration = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7231-8b23-d7c9fa41e402"),
        Module, Domain, MakersResources.accountingGeneration, MakersActions.generate,
        "Permite generar registros contables.",
        "FVP", "Contabilidad", "Generación Contabilidad", "Generar"
    );

    public static readonly MakersPermission ValidateGeneration = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7232-9d45-a1e3cb52f403"),
        Module, Domain, MakersResources.accountingGeneration, MakersActions.validate,
        "Permite validar registros contables.",
        "FVP", "Contabilidad", "Generación Contabilidad", "Validar"
    );

    public static readonly MakersPermission DeleteGeneration = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7233-8f67-f5c2db74b404"),
        Module, Domain, MakersResources.accountingGeneration, MakersActions.delete,
        "Permite eliminar generación contable.",
        "FVP", "Contabilidad", "Generación Contabilidad", "Eliminar"
    );

    public static readonly MakersPermission ViewGeneration = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7234-9c78-d8f1ae36c405"),
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

