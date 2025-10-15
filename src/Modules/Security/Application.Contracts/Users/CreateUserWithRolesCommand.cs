using Common.SharedKernel.Application.Messaging;

using Security.Application.Contracts.Roles;

namespace Security.Application.Contracts.Users;

public sealed record CreateUserWithRolesCommand(
    int Id,
    string UserName,
    string Name,
    string MiddleName,
    string Identification,
    string Email,
    string DisplayName,
    IReadOnlyList<CreateRoleCommand> Roles
) : ICommand<int>;