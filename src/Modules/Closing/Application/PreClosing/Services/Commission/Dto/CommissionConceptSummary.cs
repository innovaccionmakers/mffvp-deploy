
using Closing.Application.PreClosing.Services.Yield.Dto;
using System.Text.Json;

namespace Closing.Application.PreClosing.Services.Commission.Dto;

public class CommissionConceptSummary
{
    public CommissionConceptSummary(int commissionId, string concept, decimal value)
    {
        CommissionId = commissionId;
        Concept = concept;
        Value = value;
    }

    public int CommissionId { get; private set; }
    public string Concept { get; private set; }
    public decimal Value { get; }
}

public static class CommissionConceptSummaryExtensions
{
    public static JsonDocument ToJsonSummary(this CommissionConceptSummary summary)
    {

        var json = JsonSerializer.Serialize(new StringEntityDto(summary.CommissionId.ToString(), summary.Concept));
        return JsonDocument.Parse(json);
    }
}