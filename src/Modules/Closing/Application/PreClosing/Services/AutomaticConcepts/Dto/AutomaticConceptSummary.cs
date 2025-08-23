

using Closing.Application.PreClosing.Services.Commission.Dto;
using Closing.Application.PreClosing.Services.Yield.Dto;
using Common.SharedKernel.Domain;
using System.Text.Json;

namespace Closing.Application.PreClosing.Services.AutomaticConcepts.Dto;

public sealed record AutomaticConceptSummary(
    int ConceptId,
    string ConceptName,
    IncomeExpenseNature Nature,
    string Source,
    decimal TotalAmount
);

public static class AutomaticConceptSummaryExtensions
{
    public static JsonDocument ToJsonSummary(this AutomaticConceptSummary summary)
    {
        var json = JsonSerializer.Serialize(new StringEntityDto(summary.ConceptId.ToString(), summary.ConceptName));
        return JsonDocument.Parse(json);
    }
}