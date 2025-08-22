namespace Common.SharedKernel.Domain.Auth.Permissions;

public static class MakersPermissionFactory
{
    public static MakersPermission Create(
        Guid permissionId,
        string module, string domain, string resource, string action,
        string description,
        string? displayModule = null,
        string? displayDomain = null,
        string? displayResource = null,
        string? displayAction = null)
    {
        return new MakersPermission(
            permissionId,
            description, module, domain, resource, action,
            displayModule, displayDomain, displayResource, displayAction
        );
    }
}
