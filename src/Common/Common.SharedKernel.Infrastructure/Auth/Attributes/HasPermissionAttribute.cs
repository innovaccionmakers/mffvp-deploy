using Microsoft.AspNetCore.Authorization;

namespace Common.SharedKernel.Infrastructure.Auth.Attributes;

public class HasPermissionAttribute : AuthorizeAttribute
{
    public HasPermissionAttribute(string permission)
    {
        Policy = permission;
        AuthenticationSchemes = "JwtBearer";
    }
}