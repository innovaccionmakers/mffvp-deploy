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
        new() { Id = 1, UserId = 15828, RoleId = 2 }, // CristianVillalobos => Editor
        new() { Id = 2, UserId = 10291, RoleId = 2 }, // Cristof => Editor
        new() { Id = 3, UserId = 3, RoleId = 1 }       // Sofia => Admin
    };

    public static List<RolePermission> RolePermissions = new()
    {
        // Admin role (Id: 1) has permissions
        new() { Id = 1, RoleId = 1, PermitToken = MakersPermissionsUsers.View },
        new() { Id = 2, RoleId = 1, PermitToken = MakersPermissionsUsers.Search },
        new() { Id = 3, RoleId = 1, PermitToken = MakersPermissionsUsers.Create },
        new() { Id = 4, RoleId = 1, PermitToken = MakersPermissionsUsers.Update },

        // Editor role (Id: 2) has permissions
        new() { Id = 5, RoleId = 2, PermitToken = MakersPermissionsAssociatePensionRequirements.View },
        new() { Id = 6, RoleId = 2, PermitToken = MakersPermissionsAssociatePensionRequirements.Search },
        new() { Id = 7, RoleId = 2, PermitToken = MakersPermissionsAssociatePensionRequirements.Update }
    };

    public static List<UserPermission> UserPermissions = new()
    {
        // Cristof (Id: 10291)
        new() { Id = 1, UserId = 10291, PermitToken = MakersPermissionsUsers.Delete, Granted = true },
        new() { Id = 2, UserId = 10291, PermitToken = MakersPermissionsAssociateActivates.View, Granted = true },

        // CristianVillalobos (Id: 15828)
        new() { Id = 3, UserId = 15828, PermitToken = MakersPermissionsAssociateActivates.Update, Granted = true },
        new() { Id = 4, UserId = 15828, PermitToken = MakersPermissionsAssociateActivates.Delete, Granted = true },

        // Sofia (Id: 3)
        new() { Id = 5, UserId = 3, PermitToken = MakersPermissionsOperationsClientOperations.Search, Granted = true },
        new() { Id = 6, UserId = 3, PermitToken = MakersPermissionsOperationsClientOperations.Create, Granted = true }
    };
}
