using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Security.Application.Contracts.RolePermissions;
public sealed record class CreateRolePermissionCommand(
    int RoleId,
    string ScopePermission
) : ICommand<int>;
