using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.RolePermissions;

public sealed record class CreateRolePermissionCommand(
    int RoleId,
    string ScopePermission
) : ICommand;