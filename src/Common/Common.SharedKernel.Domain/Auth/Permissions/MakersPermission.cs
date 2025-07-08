public record MakersPermission(
    string Description,
    string Module,
    string Domain,
    string Resource,
    string Action,
    string? DisplayModule = null,
    string? DisplayDomain = null,
    string? DisplayResource = null,
    string? DisplayAction = null
)
{
    public string ScopePermission => NameFor(Module, Domain, Resource, Action);

    public string DisplayName =>
        $"{DisplayModule ?? Module}:{DisplayDomain ?? Domain}:{DisplayResource ?? Resource}:{DisplayAction ?? Action}";

    public string Name => ScopePermission;

    public static string NameFor(string module, string domain, string resource, string action)
    {
        return $"{module}:{domain}:{resource}:{action}";
    }
}
