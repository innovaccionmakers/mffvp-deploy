using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.Users;

public sealed record class AssignRolesCommand(int UserId, List<int> RolePermissionIds)
    : ICommand;