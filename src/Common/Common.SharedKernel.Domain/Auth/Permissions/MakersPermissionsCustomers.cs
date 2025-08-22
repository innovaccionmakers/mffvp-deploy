namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionsCustomers
{
    private const string Module = MakersModules.fvp;
    private const string Domain = MakersDomains.customers;

    public const string PolicyCreateCustomer = "fvp:customers:customersManagement:create";

    public static readonly MakersPermission CreateCustomer = MakersPermissionFactory.Create(
        Guid.Parse("018f1e2c-9e47-7270-9a34-b8d5c1e7a801"),
        Module, Domain, MakersResources.customersManagement, MakersActions.create,
        "Permite crear clientes.",
        "FVP", "Clientes", "Administracion de Clientes", "Crear"
    );

    public static readonly List<MakersPermission> All = new()
    {
        CreateCustomer
    };
}
