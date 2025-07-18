
using Closing.Application.PreClosing.Services.Yield.Dto;
using Closing.Domain.ProfitLosses;
using System.Text.Json;

namespace Closing.Application.PreClosing.Services.ProfitAndLoss.Dto;

public static class ProfitLossConceptSummaryExtensions
{
    public static JsonDocument ToJsonSummary(this ProfitLossConceptSummary summary)
    {

        var json = JsonSerializer.Serialize(new StringEntityDto(summary.ConceptId.ToString(), summary.ConceptName));
        return JsonDocument.Parse(json);
    }
}