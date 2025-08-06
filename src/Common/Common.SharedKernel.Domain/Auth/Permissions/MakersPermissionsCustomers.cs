namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsCustomers
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.customers;

    public const string PolicyCreateCustomer = "fvp:customers:customersManagement:create";

    public static readonly MakersPermission CreateCustomer = MakersPermissionFactory.Create(
        Module, Domain, MakersResources.customersManagement, MakersActions.create,
        "Permite crear clientes.",
        "FVP", "Clientes", "Administracion de Clientes", "Crear"
    );

    public static readonly List<MakersPermission> All = new()
    {
        CreateCustomer
    };
}
