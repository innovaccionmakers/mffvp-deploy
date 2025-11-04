namespace Security.Application.Contracts.Users;

public sealed record UserResponse(
    int Id,
    string UserName,
    string Name,
    string MiddleName,
    string Identification,
    string Email,
    string DisplayName
);
