using Common.SharedKernel.Application.Messaging;

namespace Security.Application.Contracts.Users;

public sealed record class CreatePermissionsCommand(int UserId, List<(int Token, bool Granted)> Permissions)
    : ICommand;
