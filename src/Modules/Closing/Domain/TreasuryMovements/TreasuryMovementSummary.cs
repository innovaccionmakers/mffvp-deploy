
using Closing.Domain.PreClosing;
using System.Text.Json;

namespace Closing.Domain.TreasuryMovements;

public sealed record TreasuryMovementSummary(
    long ConceptId,
    string ConceptName,
    string Nature,
    bool AllowsExpense,
    decimal TotalAmount
    )
{
    public TreasuryConceptNature? NatureEnum =>
        Enum.TryParse<TreasuryConceptNature>(Nature, ignoreCase: true, out var result)
            ? result
            : null;
}
public static class TreasuryMovementSummaryExtensions
{
    public static JsonDocument ToJsonSummary(this TreasuryMovementSummary summary)
    {

        var json = JsonSerializer.Serialize(new StringEntityDto(summary.ConceptId.ToString(), summary.ConceptName));
        return JsonDocument.Parse(json);
    }
}