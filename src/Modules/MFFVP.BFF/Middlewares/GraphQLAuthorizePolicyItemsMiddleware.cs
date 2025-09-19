using System.Reflection;
using HotChocolate.Authorization;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Http;

namespace MFFVP.BFF.Middlewares;

public sealed class GraphQLAuthorizePolicyItemsMiddleware
{
    public const string HttpContextPolicyItemKey = "Audit:Policy";

    private readonly FieldDelegate _next;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GraphQLAuthorizePolicyItemsMiddleware(FieldDelegate next, IHttpContextAccessor httpContextAccessor)
    {
        _next = next;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task InvokeAsync(IMiddlewareContext context)
    {
        string? policy = null;

        var member = context.Selection.Field?.Member;
        if (member is not null)
        {
            var methodLevelPolicies = member
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .OfType<AuthorizeAttribute>()
                .Select(a => a.Policy)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .ToArray();

            policy = methodLevelPolicies.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(policy) && member.DeclaringType is not null)
            {
                var typeLevelPolicies = member.DeclaringType
                    .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                    .OfType<AuthorizeAttribute>()
                    .Select(a => a.Policy)
                    .Where(p => !string.IsNullOrWhiteSpace(p))
                    .ToArray();

                policy = typeLevelPolicies.FirstOrDefault();
            }
        }

        if (!string.IsNullOrWhiteSpace(policy))
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext is not null)
            {
                httpContext.Items[HttpContextPolicyItemKey] = policy!;
            }
        }

        await _next(context);
    }
}

