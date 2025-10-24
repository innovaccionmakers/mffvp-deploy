using Common.SharedKernel.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace Common.SharedKernel.Infrastructure.Auth.User;

public sealed class UserService(IHttpContextAccessor httpContextAccessor) : IUserService
{
    public string GetUserName()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User.Identity?.IsAuthenticated != true)
            return "Anonymous";

        return httpContext.User.Identity?.Name ?? "Anonymous";
    }

    public string GetUserId()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User.Identity?.IsAuthenticated != true)
            return "Anonymous";
        
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value
                         ?? httpContext.User.FindFirst("user_id")?.Value
                         ?? httpContext.User.FindFirst("id")?.Value
                         ?? httpContext.User.Identity.Name;

        return userIdClaim ?? "Anonymous";
    }

    public bool IsAuthenticated()
    {
        var httpContext = httpContextAccessor.HttpContext;
        return httpContext?.User.Identity?.IsAuthenticated == true;
    }
}
