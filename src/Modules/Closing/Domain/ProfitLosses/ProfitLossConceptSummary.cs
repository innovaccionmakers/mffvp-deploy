using Closing.Domain.ProfitLossConcepts;
using System.Text.Json;

namespace Closing.Domain.ProfitLosses;

public sealed record ProfitLossConceptSummary(
    long ConceptId, 
    string ConceptName, 
    ProfitLossNature Nature, 
    string Source, 
    decimal TotalAmount
 );

public sealed record StringEntityDto(string Entity, string EntityId, string EntityValue);
public static class ProfitLossConceptSummaryExtensions
{
    public static JsonDocument ToJsonSummary(this ProfitLossConceptSummary summary)
    {
        var entities = new[]
        {
            new StringEntityDto("pyg", summary.ConceptId.ToString(),summary.ConceptName)
        };

        var json = JsonSerializer.Serialize(entities);
        return JsonDocument.Parse(json);
    }
}