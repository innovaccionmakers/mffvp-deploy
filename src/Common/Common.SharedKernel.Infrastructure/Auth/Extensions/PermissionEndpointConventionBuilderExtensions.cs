using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;

namespace Common.SharedKernel.Infrastructure.Auth.Extensions;

public static class PermissionEndpointConventionBuilderExtensions
{
    public static TBuilder HasPermission<TBuilder>(this TBuilder builder, string permission)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization(new AuthorizeAttribute
        {
            Policy = permission,
            AuthenticationSchemes = "JwtBearer"
        });
    }
}
