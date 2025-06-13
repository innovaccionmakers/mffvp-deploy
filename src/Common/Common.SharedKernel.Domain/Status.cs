using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Common.SharedKernel.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Status
{
    [EnumMember(Value = "A")]
    Active = 'A',
    [EnumMember(Value = "I")]
    Inactive = 'I'
}

