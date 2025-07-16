using Common.SharedKernel.Infrastructure.Auth.Policy;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Security.Application.Contracts.Auth;

using System.Security.Claims;

namespace Security.Infrastructure.Auth;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IHttpContextAccessor _accessor;
    private readonly IUserPermissionService _permissionService;

    public PermissionHandler(IHttpContextAccessor accessor, IUserPermissionService permissionService)
    {
        _accessor = accessor;
        _permissionService = permissionService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var httpContext = _accessor.HttpContext;
        var userName = httpContext?.User?.Identity?.Name;

        if (!string.IsNullOrWhiteSpace(userName))
        {
            var permissions = await _permissionService.GetPermissionsByUserNameAsync(userName);
            if (permissions.Contains(requirement.Token))
                context.Succeed(requirement);
        }
    }
}