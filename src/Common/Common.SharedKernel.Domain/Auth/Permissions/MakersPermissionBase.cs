namespace Common.SharedKernel.Domain.Auth.Permissions;

public abstract class MakersPermissionBase
{
    public Guid PermissionId { get; protected set; }
    public string Description { get; protected set; }
    public string Module { get; protected set; }
    public string Domain { get; protected set; }
    public string Resource { get; protected set; }
    public string Action { get; protected set; }
    public string? DisplayModule { get; protected set; }
    public string? DisplayDomain { get; protected set; }
    public string? DisplayResource { get; protected set; }
    public string? DisplayAction { get; protected set; }

    protected MakersPermissionBase(
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
    {
        PermissionId = permissionId;
        Description = description;
        Module = module;
        Domain = domain;
        Resource = resource;
        Action = action;
        DisplayModule = displayModule;
        DisplayDomain = displayDomain;
        DisplayResource = displayResource;
        DisplayAction = displayAction;
    }

    public abstract string ScopePermission { get; }


    public virtual string DisplayName =>
        $"{DisplayModule ?? Module}:{DisplayDomain ?? Domain}:{DisplayResource ?? Resource}:{DisplayAction ?? Action}";

    public string Name => ScopePermission;
}

