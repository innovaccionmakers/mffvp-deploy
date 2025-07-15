using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Common.SharedKernel.Domain;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum IncomeExpenseNature
{
    [EnumMember(Value = "Ingreso")]
    Income,
    [EnumMember(Value = "Gasto")]
    Expense,
}
