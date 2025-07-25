﻿namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsReports
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.reports;

    public static readonly MakersPermission GenerateBalances = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.reportBalancesAndMovements, MakersActions.generate,
        "Permite generar el informe de saldos y movimientos.",
        "FVP", "Informes", "Saldos y Movimientos", "Generar"
    );

    public static readonly MakersPermission GenerateTransfers = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.reportTransfers, MakersActions.generate,
        "Permite generar el informe de transmisiones.",
        "FVP", "Informes", "Transmisiones", "Generar"
    );

    public static readonly List<MakersPermission> All = new()
    {
        GenerateBalances,
        GenerateTransfers
    };
}
