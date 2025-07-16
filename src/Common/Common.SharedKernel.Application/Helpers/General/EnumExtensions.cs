
using System.Reflection;
using System.Runtime.Serialization;

namespace Common.SharedKernel.Application.Helpers.General;
public static class EnumExtensions
{
    public static TEnum? ParseEnumMemberValue<TEnum>(string? value) where TEnum : struct, Enum
    {
        if (value is null) return null;

        foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var attr = field.GetCustomAttribute<EnumMemberAttribute>();
            if (attr?.Value == value)
                return (TEnum)field.GetValue(null)!;
        }

        return null;
    }
}