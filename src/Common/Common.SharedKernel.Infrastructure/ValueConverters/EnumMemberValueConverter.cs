using System.Reflection;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Common.SharedKernel.Infrastructure.ValueConverters;

public sealed class EnumMemberValueConverter<TEnum> : ValueConverter<TEnum,string>
    where TEnum : struct, Enum
{
    private static readonly IReadOnlyDictionary<TEnum,string> ToDb;
    private static readonly IReadOnlyDictionary<string,TEnum> FromDb;

    static EnumMemberValueConverter()
    {
        var vals = Enum.GetValues(typeof(TEnum)).Cast<TEnum>();

        ToDb   = vals.ToDictionary(v => v, GetEnumMemberValue);
        FromDb = ToDb.ToDictionary(kv => kv.Value, kv => kv.Key,
            StringComparer.OrdinalIgnoreCase);
    }
    
    private static TEnum FromProvider(string dbValue)
    {
        if (FromDb.TryGetValue(dbValue, out var e))
            return e;

        throw new InvalidOperationException(
            $"Valor «{dbValue}» no es válido para {typeof(TEnum).Name}.");
    }

    public EnumMemberValueConverter()
        : base(
            v => ToDb[v],
            v => FromProvider(v))
    { }

    private static string GetEnumMemberValue(TEnum value) =>
        typeof(TEnum)
            .GetMember(value.ToString())[0]
            .GetCustomAttribute<EnumMemberAttribute>()?.Value
        ?? value.ToString();

    public static readonly EnumMemberValueConverter<TEnum> Instance = new();
}
