namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsAccounting
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.accounting;
    private const string AccountingConfigurationResource = MakersResources.accountingConfiguration;


    public const string PolicyGenerateGeneration = "fvp:accounting:accountingGeneration:generate";


    public const string PolicyCreateConsecutive = "fvp:accounting:accountingConfiguration:consecutives:create";


    public const string PolicyCreateConcept = "fvp:accounting:accountingConfiguration:concepts:create";
    public const string PolicyViewConcept = "fvp:accounting:accountingConfiguration:concepts:view";
    public const string PolicyUpdateConcept = "fvp:accounting:accountingConfiguration:concepts:update";
    public const string PolicyDeleteConcept = "fvp:accounting:accountingConfiguration:concepts:delete";


    public const string PolicyCreateTreasury = "fvp:accounting:accountingConfiguration:treasury:create";
    public const string PolicyViewTreasury = "fvp:accounting:accountingConfiguration:treasury:view";
    public const string PolicyUpdateTreasury = "fvp:accounting:accountingConfiguration:treasury:update";
    public const string PolicyDeleteTreasury = "fvp:accounting:accountingConfiguration:treasury:delete";


    public const string PolicyCreatePassiveTransaction = "fvp:accounting:accountingConfiguration:passiveTransactions:create";
    public const string PolicyViewPassiveTransaction = "fvp:accounting:accountingConfiguration:passiveTransactions:view";
    public const string PolicyUpdatePassiveTransaction = "fvp:accounting:accountingConfiguration:passiveTransactions:update";
    public const string PolicyDeletePassiveTransaction = "fvp:accounting:accountingConfiguration:passiveTransactions:delete";


    public const string PolicyCreateGeneralConfiguration = "fvp:accounting:accountingConfiguration:generalConfiguration:create";
    public const string PolicyViewGeneralConfiguration = "fvp:accounting:accountingConfiguration:generalConfiguration:view";
    public const string PolicyUpdateGeneralConfiguration = "fvp:accounting:accountingConfiguration:generalConfiguration:update";
    public const string PolicyDeleteGeneralConfiguration = "fvp:accounting:accountingConfiguration:generalConfiguration:delete";


    public static readonly MakersPermission GenerateGeneration = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7231-8b23-d7c9fa41e402"),
        Module, Domain, MakersResources.accountingGeneration, MakersActions.generate,
        "Permite generar registros contables.",
        "FVP", "Contabilidad", "Generación Contabilidad", "Generar"
    );
    
    public static readonly MakersPermissionWithSubResource CreateConsecutive = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("275a4f7e-b774-46ae-a5ba-68056cf8f8a3"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.consecutives, MakersActions.create,
        "Permite crear y consultar consecutivos contables.",
        "FVP", "Configuración Contabilidad", "Consecutivos", "Crear"
    );
    
    public static readonly MakersPermissionWithSubResource CreateConcept = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("3a8b9c1d-2e3f-4a5b-6c7d-8e9f0a1b2c3d"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.concepts, MakersActions.create,
        "Permite crear conceptos contables.",
        "FVP", "Configuración Contabilidad", "Conceptos", "Crear"
    );

    public static readonly MakersPermissionWithSubResource ViewConcept = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("4b9c0d1e-3f4a-5b6c-7d8e-9f0a1b2c3d4e"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.concepts, MakersActions.view,
        "Permite consultar conceptos contables.",
        "FVP", "Configuración Contabilidad", "Conceptos", "Consultar"
    );

    public static readonly MakersPermissionWithSubResource UpdateConcept = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("5c0d1e2f-4a5b-6c7d-8e9f-0a1b2c3d4e5f"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.concepts, MakersActions.update,
        "Permite editar conceptos contables.",
        "FVP", "Configuración Contabilidad", "Conceptos", "Editar"
    );

    public static readonly MakersPermissionWithSubResource DeleteConcept = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("6d1e2f3a-5b6c-7d8e-9f0a-1b2c3d4e5f6a"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.concepts, MakersActions.delete,
        "Permite eliminar conceptos contables.",
        "FVP", "Configuración Contabilidad", "Conceptos", "Eliminar"
    );
    
    public static readonly MakersPermissionWithSubResource CreateTreasury = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("7e2f3a4b-6c7d-8e9f-0a1b-2c3d4e5f6a7b"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.treasury, MakersActions.create,
        "Permite crear configuraciones de tesorería.",
        "FVP", "Configuración Contabilidad", "Tesorería", "Crear"
    );

    public static readonly MakersPermissionWithSubResource ViewTreasury = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("8f3a4b5c-7d8e-9f0a-1b2c-3d4e5f6a7b8c"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.treasury, MakersActions.view,
        "Permite consultar configuraciones de tesorería.",
        "FVP", "Configuración Contabilidad", "Tesorería", "Consultar"
    );

    public static readonly MakersPermissionWithSubResource UpdateTreasury = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("9a4b5c6d-8e9f-0a1b-2c3d-4e5f6a7b8c9d"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.treasury, MakersActions.update,
        "Permite editar configuraciones de tesorería.",
        "FVP", "Configuración Contabilidad", "Tesorería", "Editar"
    );

    public static readonly MakersPermissionWithSubResource DeleteTreasury = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("0b5c6d7e-9f0a-1b2c-3d4e-5f6a7b8c9d0e"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.treasury, MakersActions.delete,
        "Permite eliminar configuraciones de tesorería.",
        "FVP", "Configuración Contabilidad", "Tesorería", "Eliminar"
    );
   
    public static readonly MakersPermissionWithSubResource CreatePassiveTransaction = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("1c6d7e8f-0a1b-2c3d-4e5f-6a7b8c9d0e1f"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.passiveTransactions, MakersActions.create,
        "Permite crear configuraciones de transacciones pasivas.",
        "FVP", "Configuración Contabilidad", "Transacciones Pasivas", "Crear"
    );

    public static readonly MakersPermissionWithSubResource ViewPassiveTransaction = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("2d7e8f9a-1b2c-3d4e-5f6a-7b8c9d0e1f2a"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.passiveTransactions, MakersActions.view,
        "Permite consultar configuraciones de transacciones pasivas.",
        "FVP", "Configuración Contabilidad", "Transacciones Pasivas", "Consultar"
    );

    public static readonly MakersPermissionWithSubResource UpdatePassiveTransaction = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("3e8f9a0b-2c3d-4e5f-6a7b-8c9d0e1f2a3b"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.passiveTransactions, MakersActions.update,
        "Permite editar configuraciones de transacciones pasivas.",
        "FVP", "Configuración Contabilidad", "Transacciones Pasivas", "Editar"
    );

    public static readonly MakersPermissionWithSubResource DeletePassiveTransaction = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("4f9a0b1c-3d4e-5f6a-7b8c-9d0e1f2a3b4c"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.passiveTransactions, MakersActions.delete,
        "Permite eliminar configuraciones de transacciones pasivas.",
        "FVP", "Configuración Contabilidad", "Transacciones Pasivas", "Eliminar"
    );

   
    public static readonly MakersPermissionWithSubResource CreateGeneralConfiguration = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("5a0b1c2d-4e5f-6a7b-8c9d-0e1f2a3b4c5d"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.generalConfiguration, MakersActions.create,
        "Permite crear configuraciones generales de contabilidad.",
        "FVP", "Configuración Contabilidad", "Configuración General", "Crear"
    );

    public static readonly MakersPermissionWithSubResource ViewGeneralConfiguration = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("6b1c2d3e-5f6a-7b8c-9d0e-1f2a3b4c5d6e"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.generalConfiguration, MakersActions.view,
        "Permite consultar configuraciones generales de contabilidad.",
        "FVP", "Configuración Contabilidad", "Configuración General", "Consultar"
    );

    public static readonly MakersPermissionWithSubResource UpdateGeneralConfiguration = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("7c2d3e4f-6a7b-8c9d-0e1f-2a3b4c5d6e7f"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.generalConfiguration, MakersActions.update,
        "Permite editar configuraciones generales de contabilidad.",
        "FVP", "Configuración Contabilidad", "Configuración General", "Editar"
    );

    public static readonly MakersPermissionWithSubResource DeleteGeneralConfiguration = MakersPermissionFactory.CreateWithSubResource(
        Guid.Parse("8d3e4f5a-7b8c-9d0e-1f2a-3b4c5d6e7f8a"),
        Module, Domain, AccountingConfigurationResource, MakersSubResources.generalConfiguration, MakersActions.delete,
        "Permite eliminar configuraciones generales de contabilidad.",
        "FVP", "Configuración Contabilidad", "Configuración General", "Eliminar"
    );

    public static readonly List<MakersPermissionBase> All = new()
    {
        GenerateGeneration,
        CreateConsecutive,
        CreateConcept,
        ViewConcept,
        UpdateConcept,
        DeleteConcept,
        CreateTreasury,
        ViewTreasury,
        UpdateTreasury,
        DeleteTreasury,
        CreatePassiveTransaction,
        ViewPassiveTransaction,
        UpdatePassiveTransaction,
        DeletePassiveTransaction,
        CreateGeneralConfiguration,
        ViewGeneralConfiguration,
        UpdateGeneralConfiguration,
        DeleteGeneralConfiguration
    };
}

