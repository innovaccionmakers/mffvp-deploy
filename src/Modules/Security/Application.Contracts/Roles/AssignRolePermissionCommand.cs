using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.Roles;

public sealed record class AssignRolePermissionCommand(
    int RoleId,
    List<string> ScopePermissions
) : ICommand;
