using Common.SharedKernel.Domain.Auth;
using Common.SharedKernel.Domain.Auth.Permissions;

namespace Common.SharedKernel.Application.Auth;

public static class SeedData
{
    public static List<User> Users = new()
    {
        new() { Id = 15828, UserName = "CristianVillalobos", DisplayName = "Cristian Villalobos" },
        new() { Id = 10291, UserName = "Cristof", DisplayName = "Cristof Lora" },
        new() { Id = 3, UserName = "sofia", DisplayName = "Sofía Ortega" }
    };

    public static List<Role> Roles = new()
    {
        new() { Id = 1, Name = "Admin", Objective = "Admin general" },
        new() { Id = 2, Name = "Editor", Objective = "Data editor" }
    };

    public static List<UserRole> UserRoles = new()
    {
        new() { Id = 1, UserId = 1, RoleId = 2 }, // CristianVillalobos => Editor
        new() { Id = 2, UserId = 2, RoleId = 2 }, // Cristof => Editor
        new() { Id = 3, UserId = 3, RoleId = 1 }  // Sofia => Admin
    };

    public static List<RolePermission> RolePermissions = new()
    {
        // Admin role (Id: 1) has permissions
        new() { Id = 1, RoleId = 1, PermitToken = MakersPermissions.All[0].Name },  // View Users
        new() { Id = 2, RoleId = 1, PermitToken = MakersPermissions.All[1].Name },  // Search Users
        new() { Id = 3, RoleId = 1, PermitToken = MakersPermissions.All[2].Name },  // Create Users
        new() { Id = 4, RoleId = 1, PermitToken = MakersPermissions.All[3].Name },  // Update Users

        // Editor role (Id: 2) has permissions
        new() { Id = 5, RoleId = 2, PermitToken = MakersPermissions.All[10].Name }, // View Pension Requirements
        new() { Id = 6, RoleId = 2, PermitToken = MakersPermissions.All[11].Name }, // Search Pension Requirements
        new() { Id = 7, RoleId = 2, PermitToken = MakersPermissions.All[13].Name }  // Update Pension Requirements
    };

    public static List<UserPermission> UserPermissions = new()
    {
        // Cristof (Id: 2)
        new() { Id = 1, UserId = 2, PermitToken = MakersPermissions.All[4].Name, Granted = true },  // Delete Users
        new() { Id = 2, UserId = 2, PermitToken = MakersPermissions.All[5].Name, Granted = true },  // View Activates

        // CristianVillalobos (Id: 1)
        new() { Id = 3, UserId = 1, PermitToken = MakersPermissions.All[15].Name, Granted = true }, // Update Activates
        new() { Id = 4, UserId = 1, PermitToken = MakersPermissions.All[16].Name, Granted = true }, // Delete Activates

        // Sofia (Id: 3)
        new() { Id = 5, UserId = 3, PermitToken = MakersPermissions.All[25].Name, Granted = true }, // Search Client Operations
        new() { Id = 6, UserId = 3, PermitToken = MakersPermissions.All[26].Name, Granted = true }  // Create Client Operations
    };
}
