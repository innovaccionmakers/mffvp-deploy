using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.Roles;

public sealed record class CreateRoleCommand(
    int Id,
    string Name,
    string Objective
) : ICommand;
