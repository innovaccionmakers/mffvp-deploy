using System.Reflection;
using System.Runtime.Serialization;

namespace Common.SharedKernel.Application.Helpers.Serialization;

public static class EnumHelper
{
    public static string GetEnumMemberValue(Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        var attribute = field?.GetCustomAttribute<EnumMemberAttribute>();
        return attribute?.Value ?? value.ToString();
    }
}
