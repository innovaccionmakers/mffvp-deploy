namespace Common.SharedKernel.Domain.Auth.Permissions;

public record MakersPermission(string Description, string Module, string Domain, string Resource, string Action)
{
    public string Name => NameFor(Module, Domain, Action, Resource);
    public static string NameFor(string module, string domain, string action, string resource)
    {
        return $"{module}:{domain}:{resource}:{action}";
    }
}
