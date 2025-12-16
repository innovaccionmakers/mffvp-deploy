namespace Common.SharedKernel.Domain.Auth.Permissions;

public class MakersPermission : MakersPermissionBase
{
    public MakersPermission(
        Guid permissionId,
        string description,
        string module,
        string domain,
        string resource,
        string action,
        string? displayModule = null,
        string? displayDomain = null,
        string? displayResource = null,
        string? displayAction = null)
        : base(permissionId, description, module, domain, resource, action,
              displayModule, displayDomain, displayResource, displayAction)
    {
    }

    public override string ScopePermission => NameFor(Module, Domain, Resource, Action);

    public static string NameFor(string module, string domain, string resource, string action)
    {
        return $"{module}:{domain}:{resource}:{action}";
    }
}
