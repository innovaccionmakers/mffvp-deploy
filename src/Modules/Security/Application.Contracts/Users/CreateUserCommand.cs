using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.Users;

public sealed record CreateUserCommand(
    int Id,
    string UserName,
    string Name,
    string MiddleName,
    string Identification,
    string Email,
    string DisplayName
) : ICommand<int>;
