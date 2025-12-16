namespace Common.SharedKernel.Domain.Auth.Permissions;

public class MakersPermissionWithSubResource : MakersPermissionBase
{
    public string SubResource { get; protected set; }
    public string? DisplaySubResource { get; protected set; }

    public MakersPermissionWithSubResource(
        Guid permissionId,
        string description,
        string module,
        string domain,
        string resource,
        string subResource,
        string action,
        string? displayModule = null,
        string? displayDomain = null,
        string? displayResource = null,
        string? displaySubResource = null,
        string? displayAction = null)
        : base(permissionId, description, module, domain, resource, action,
              displayModule, displayDomain, displayResource, displayAction)
    {
        SubResource = subResource;
        DisplaySubResource = displaySubResource;
    }

    public override string ScopePermission => NameFor(Module, Domain, Resource, SubResource, Action);

    public override string DisplayName =>
        $"{DisplayModule ?? Module}:{DisplayDomain ?? Domain}:{DisplayResource ?? Resource}:{DisplaySubResource ?? SubResource}:{DisplayAction ?? Action}";

    public static string NameFor(string module, string domain, string resource, string subResource, string action)
    {
        return $"{module}:{domain}:{resource}:{subResource}:{action}";
    }
}

