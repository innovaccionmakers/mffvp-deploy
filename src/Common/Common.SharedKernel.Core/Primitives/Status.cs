using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Common.SharedKernel.Core.Primitives;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    [EnumMember(Value = "A")]
    Active = 'A',
    [EnumMember(Value = "I")]
    Inactive = 'I'
}
