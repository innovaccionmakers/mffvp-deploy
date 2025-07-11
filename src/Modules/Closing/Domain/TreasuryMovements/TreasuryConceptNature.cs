using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Closing.Domain.TreasuryMovements;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TreasuryConceptNature
{
    [EnumMember(Value = "Ingreso")]
    Income,
    [EnumMember(Value = "Gasto")]
    Expense,
}