using Common.SharedKernel.Domain.Auth;

namespace Common.SharedKernel.Application.Auth;


public static class SeedData
{
    public static List<User> Users = new()
    {
        new() { Id = 1, UserName = "Cristof", DisplayName = "Cristian Villalobos" },
        new() { Id = 2, UserName = "leo", DisplayName = "Leonardo Ruiz" },
        new() { Id = 3, UserName = "sofia", DisplayName = "Sofía Ortega" }
    };

    public static List<Role> Roles = new()
    {
        new() { Id = 1, Name = "Admin", Objective = "Admin general" },
        new() { Id = 2, Name = "Editor", Objective = "Data editor" }
    };

    public static List<UserRole> UserRoles = new()
    {
        new() { Id = 1, UserId = 1, RoleId = 2 },
        new() { Id = 2, UserId = 2, RoleId = 2 },
        new() { Id = 3, UserId = 3, RoleId = 1 }
    };

    public static List<RolePermission> RolePermissions = new()
    {
        new() { Id = 1, RoleId = 1, PermitToken = "Permission.MasterData:Core:Modify" },
        new() { Id = 2, RoleId = 1, PermitToken = "Permission.MasterData:Core:Delete" },
        new() { Id = 3, RoleId = 2, PermitToken = "Permission.MasterData:Insumo:Read" }
    };

    public static List<UserPermission> UserPermissions = new()
    {
        new() { Id = 1, UserId = 2, PermitToken = "Permission.MasterData:Core:Delete", Granted = true },
        new() { Id = 2, UserId = 1, PermitToken = "Permission.MasterData:Insumo:Modify", Granted = true }
    };
}