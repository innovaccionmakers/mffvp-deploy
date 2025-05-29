using Common.SharedKernel.Application.Auth;
using Common.SharedKernel.Domain.Auth;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Common.SharedKernel.Infrastructure.Auth.Policy;

public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IHttpContextAccessor _accessor;

    public PermissionHandler(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var httpContext = _accessor.HttpContext;
        var username = httpContext.User.Identity?.Name;
        if (username != null)
        {
            if (PermissionEvaluator.HasPermission(username, requirement.Token))
                context.Succeed(requirement);
        }
        return Task.CompletedTask;
    }
}