using Security.Application.Contracts.Users;

namespace Security.IntegrationEvents.Users.GetByUsername;

public sealed record GetUserByUsernameResponse(
    bool Succeeded,
    UserResponse? User,
    string? Code,
    string? Message 
);