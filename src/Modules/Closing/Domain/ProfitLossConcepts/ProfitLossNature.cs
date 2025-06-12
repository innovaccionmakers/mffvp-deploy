using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Closing.Domain.ProfitLossConcepts;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ProfitLossNature
{
    [EnumMember(Value = "Ingreso")]
    Income,
    [EnumMember(Value = "Gasto")]
    Expense,
}