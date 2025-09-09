using Closing.Integrations.Common;
using System.Text.Json.Serialization;

namespace Closing.Integrations.Closing.RunClosing;
public sealed class ConfirmClosingResult
{
    public ConfirmClosingResult(int portfolioId, DateTime closingDate)
    {
        PortfolioId = portfolioId;
        ClosingDate = closingDate;
    }
    [property: JsonPropertyName("IdPortafolio")]
    public int PortfolioId { get; init; }
    [property: JsonPropertyName("FechaCierre")]
    public DateTime ClosingDate { get; init; }

    [property: JsonPropertyName("TieneAdvertencias")]
    public bool? HasWarnings { get; set; }

    [property: JsonPropertyName("Advertencias")]
    public IReadOnlyList<WarningItem>? Warnings { get; set; }
}