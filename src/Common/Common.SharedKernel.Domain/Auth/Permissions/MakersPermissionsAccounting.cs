namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsAccounting
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.accounting;

    public const string PolicyGenerateGeneration = "fvp:accounting:accountingGeneration:generate";
    public const string PolicyCreateAccountingConsecutive = "fvp:accounting:accountingConsecutives:create";

    public static readonly MakersPermission GenerateGeneration = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7231-8b23-d7c9fa41e402"),
        Module, Domain, MakersResources.accountingGeneration, MakersActions.generate,
        "Permite generar registros contables.",
        "FVP", "Contabilidad", "Generación Contabilidad", "Generar"
    );

    public static readonly MakersPermission CreateAccountingConsecutive = MakersPermissionFactory.Create(
        Guid.Parse("275a4f7e-b774-46ae-a5ba-68056cf8f8a3"),
        Module, Domain, MakersResources.accountingConsecutives, MakersActions.create,
        "Permite actualizar y consultar consecutivos contables.",
        "FVP", "Configuración Contabilidad", "Consecutivos", "Crear"
    );

    public static readonly List<MakersPermission> All = new()
    {
        GenerateGeneration,
        CreateAccountingConsecutive
    };
}

