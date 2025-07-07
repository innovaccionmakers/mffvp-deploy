using Common.SharedKernel.Infrastructure.Auth.Policy;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Security.Application.Contracts.Auth;

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
        var userIdClaim = httpContext?.User?.FindFirst("sub")?.Value;

        if (int.TryParse(userIdClaim, out var userId))
        {
            var permissions = await _permissionService.GetPermissionsAsync(userId);
            if (permissions.Contains(requirement.Token))
                context.Succeed(requirement);
        }
    }
}