using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.Roles;

public sealed record CreateRoleCommand(
    int Id,
    string Name,
    string Objective
) : ICommand<int>;