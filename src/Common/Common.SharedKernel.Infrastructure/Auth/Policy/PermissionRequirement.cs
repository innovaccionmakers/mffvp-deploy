using Microsoft.AspNetCore.Authorization;

namespace Common.SharedKernel.Infrastructure.Auth.Policy;

public class PermissionRequirement : IAuthorizationRequirement
{
    public string Token { get; }

    public PermissionRequirement(string token) => Token = token;
}
