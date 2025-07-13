using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.UserRoles;

public sealed record class UpdateUserRolesCommand(
    int UserId,
    List<int> RoleIds
) : ICommand;
