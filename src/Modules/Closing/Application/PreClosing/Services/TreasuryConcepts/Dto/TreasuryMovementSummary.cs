
using Closing.Application.PreClosing.Services.Yield.Dto;
using Common.SharedKernel.Application.Helpers.General;
using Common.SharedKernel.Domain;
using System.Text.Json;

namespace Closing.Application.PreClosing.Services.TreasuryConcepts.Dto;

public sealed record TreasuryMovementSummary(
    long ConceptId,
    string ConceptName,
    string Nature,
    bool AllowsExpense,
    decimal TotalAmount
    )
{
    public IncomeExpenseNature? NatureEnum => EnumExtensions.ParseEnumMemberValue<IncomeExpenseNature>(Nature);
}
public static class TreasuryMovementSummaryExtensions
{
    public static JsonDocument ToJsonSummary(this TreasuryMovementSummary summary)
    {

        var json = JsonSerializer.Serialize(new StringEntityDto(summary.ConceptId.ToString(), summary.ConceptName));
        return JsonDocument.Parse(json);
    }
}