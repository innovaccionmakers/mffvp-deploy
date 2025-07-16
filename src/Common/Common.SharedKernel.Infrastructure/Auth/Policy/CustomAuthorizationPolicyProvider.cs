using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Common.SharedKernel.Infrastructure.Auth.Policy;

public class CustomAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

    public override Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();

        return Task.FromResult(policy);
    }
}