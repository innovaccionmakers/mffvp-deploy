using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Products.Integrations.Objectives.GetObjectives;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum StatusType
{
    [EnumMember(Value = "A")]
    A = 'A',
    [EnumMember(Value = "I")]
    I = 'I',
    [EnumMember(Value = "T")]
    T = 'T',
    Unknown = '?',
    
    Missing = 0,
}