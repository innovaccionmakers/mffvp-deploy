namespace Common.SharedKernel.Application.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class AuditLogAttribute : Attribute
{
    public string Description { get; } = "Undefined action";
}