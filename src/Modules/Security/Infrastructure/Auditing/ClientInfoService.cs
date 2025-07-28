using Microsoft.AspNetCore.Http;
using System.Linq;
using Security.Application.Abstractions.Services.Auditing;

namespace Security.Infrastructure.Auditing;

internal sealed class ClientInfoService(IHttpContextAccessor httpContextAccessor) : IClientInfoService
{
    public string GetClientIpAddress()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return "N/A";

        var forwarded = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwarded))
        {
            var ips = forwarded.Split(',');
            return ips[0].Trim();
        }

        return httpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A";
    }

    public string GetUserName()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null || httpContext.User.Identity?.IsAuthenticated != true)
            return "Anonymous";

        return httpContext.User.Identity?.Name ?? "Anonymous";
    }

    public string GetActionDescription()
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
            return "Unknown action";

        var actionClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == "action")?.Value;
        return string.IsNullOrWhiteSpace(actionClaim) ? "Unknown action" : actionClaim;
    }
}