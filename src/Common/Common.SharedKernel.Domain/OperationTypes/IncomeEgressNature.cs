using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Common.SharedKernel.Domain.OperationTypes;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IncomeEgressNature
{
    [EnumMember(Value = "I")]
    Income,
    [EnumMember(Value = "E")]
    Egress
}
