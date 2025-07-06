using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.RolePermissions;

public sealed record class DeleteRolePermissionCommand(
    int RolePermissionId
) : ICommand;
