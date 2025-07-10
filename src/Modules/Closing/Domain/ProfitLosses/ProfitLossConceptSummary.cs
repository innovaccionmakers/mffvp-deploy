using Closing.Domain.PreClosing;
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


public static class ProfitLossConceptSummaryExtensions
{
    public static JsonDocument ToJsonSummary(this ProfitLossConceptSummary summary)
    {

        var json = JsonSerializer.Serialize(new StringEntityDto(summary.ConceptId.ToString(), summary.ConceptName));
        return JsonDocument.Parse(json);
    }
}