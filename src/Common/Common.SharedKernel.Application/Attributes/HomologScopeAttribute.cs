namespace Common.SharedKernel.Application.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public sealed class HomologScopeAttribute : Attribute
{
    public HomologScopeAttribute(string scope) => Scope = scope;
    public string Scope { get; }
}