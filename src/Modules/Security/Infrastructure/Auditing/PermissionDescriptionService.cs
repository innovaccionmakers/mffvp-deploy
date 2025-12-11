using System.Reflection;
using System.Linq;
using Common.SharedKernel.Domain.Auth.Permissions;
using Security.Application.Abstractions.Services.Auditing;

namespace Security.Infrastructure.Auditing;

internal sealed class PermissionDescriptionService : IPermissionDescriptionService
{
    private readonly Dictionary<string, string> _policyDescriptions;
    private readonly IReadOnlyCollection<MakersPermissionBase> _permissions;

    public PermissionDescriptionService()
    {
        _permissions = LoadAllPermissions();
        _policyDescriptions = _permissions.ToDictionary(p => p.ScopePermission, p => p.Description);
    }

    private static IReadOnlyCollection<MakersPermissionBase> LoadAllPermissions()
    {
        var permissionType = typeof(MakersPermissionBase);
        var assembly = permissionType.Assembly;

        var permissions = assembly
            .GetTypes()
            .Where(t => t.IsClass && t.Name.StartsWith("MakersPermissions", StringComparison.Ordinal))
            .Select(t => t.GetField("All", BindingFlags.Public | BindingFlags.Static))
            .Where(f => f is not null)
            .SelectMany(f =>
            {
                var value = f!.GetValue(null);
                if (value is IEnumerable<MakersPermissionBase> basePermissions)
                    return basePermissions;
                if (value is IEnumerable<MakersPermission> simplePermissions)
                    return simplePermissions.Cast<MakersPermissionBase>();
                if (value is IEnumerable<MakersPermissionWithSubResource> subResourcePermissions)
                    return subResourcePermissions.Cast<MakersPermissionBase>();
                return Enumerable.Empty<MakersPermissionBase>();
            })
            .ToList();

        return permissions;
    }

    public string? GetDescriptionByPolicy(string policy)
    {
        return _policyDescriptions.TryGetValue(policy, out var description) ? description : null;
    }

    public string? GetDescriptionByEndpoint(string endpointName, string httpMethod)
    {
        var matches = _permissions
            .Where(p => string.Equals(p.DisplayResource, endpointName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matches.Count == 0)
        {
            return null;
        }

        if (matches.Count == 1)
        {
            return matches[0].Description;
        }

        var filtered = matches.Where(p => MatchesHttpMethod(p.Action, httpMethod)).ToList();

        return filtered.FirstOrDefault()?.Description ?? matches.First().Description;
    }

    private static bool MatchesHttpMethod(string action, string httpMethod)
    {
        return httpMethod switch
        {
            "GET" => action.Equals(MakersActions.view, StringComparison.OrdinalIgnoreCase) ||
                     action.Equals(MakersActions.search, StringComparison.OrdinalIgnoreCase),
            "POST" => action.Equals(MakersActions.create, StringComparison.OrdinalIgnoreCase) ||
                      action.Equals(MakersActions.execute, StringComparison.OrdinalIgnoreCase) ||
                      action.Equals(MakersActions.activate, StringComparison.OrdinalIgnoreCase) ||
                      action.Equals(MakersActions.process, StringComparison.OrdinalIgnoreCase) ||
                      action.Equals(MakersActions.generate, StringComparison.OrdinalIgnoreCase),
            "PUT" or "PATCH" => action.Equals(MakersActions.update, StringComparison.OrdinalIgnoreCase) ||
                                   action.Equals(MakersActions.validate, StringComparison.OrdinalIgnoreCase),
            "DELETE" => action.Equals(MakersActions.delete, StringComparison.OrdinalIgnoreCase),
            _ => false
        };
    }
}