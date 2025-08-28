namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsReports
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.reports;

    public const string PolicyGenerateBalancesAndMovements = "fvp:reports:reportBalancesAndMovements:generate";
    public const string PolicyGenerateTransfers = "fvp:reports:reportTransfers:generate";
    public const string PolicyValidateTransfers = "fvp:reports:reportTransfers:validate";
    public const string PolicyGenerateDeposits = "fvp:reports:reportDeposits:generate";
    public const string PolicyGenerateTechnicalSheet = "fvp:reports:reportTechnicalSheet:generate";

    public static readonly MakersPermission GenerateBalances = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7290-9e56-b7c3a9d4aa01"),
        Module, Domain, MakersResources.reportBalancesAndMovements, MakersActions.generate,
        "Permite generar el informe de saldos y movimientos.",
        "FVP", "Informes", "Saldos y Movimientos", "Generar"
    );

    public static readonly MakersPermission GenerateTransfers = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7291-8d67-c1f5dbb2ab02"),
        Module, Domain, MakersResources.reportTransfers, MakersActions.generate,
        "Permite generar el informe de transmisiones.",
        "FVP", "Informes", "Transmisiones", "Generar"
    );

    public static readonly MakersPermission ValidateTransfers = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7294-9a12-b3c4d5e6fa04"),
        Module, Domain, MakersResources.reportTransfers, MakersActions.validate,
        "Permite validar el informe de transmisiones.",
        "FVP", "Informes", "Transmisiones", "Validar"
    );

    public static readonly MakersPermission GenerateDeposits = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7292-9c7a-d4e5f6a7b803"),
        Module, Domain, MakersResources.reportDeposits, MakersActions.generate,
        "Permite generar el informe de depósitos.",
        "FVP", "Informes", "Informe Depósitos", "Generar"
    );

    public static readonly MakersPermission GenerateTechnicalSheet = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7293-8f21-c3d5e7f9ab03"),
        Module, Domain, MakersResources.reportTechnicalSheet, MakersActions.generate,
        "Permite generar la ficha técnica.",
        "FVP", "Informes", "Ficha Técnica", "Generar"
    );



    public static readonly List<MakersPermission> All = new()
    {
        GenerateBalances,
        GenerateTransfers
    };
}
