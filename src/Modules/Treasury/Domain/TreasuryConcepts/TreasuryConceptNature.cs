using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Treasury.Domain.TreasuryConcepts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TreasuryConceptNature
{
    [EnumMember(Value = "Ingreso")]
    Income,
    [EnumMember(Value = "Gasto")]
    Expense,
}