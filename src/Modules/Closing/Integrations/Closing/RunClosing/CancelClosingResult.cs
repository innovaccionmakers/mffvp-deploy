using Closing.Integrations.Common;
using System.Text.Json.Serialization;

namespace Closing.Integrations.Closing.RunClosing;
public sealed class CancelClosingResult
{
    public CancelClosingResult(int portfolioId, DateTime closingDate, bool? isCanceled)
    {
        PortfolioId = portfolioId;
        ClosingDate = closingDate;
        IsCanceled = isCanceled;
    }
    [property: JsonPropertyName("IdPortafolio")]
    public int PortfolioId { get; init; }
    [property: JsonPropertyName("FechaCierre")]
    public DateTime ClosingDate { get; init; }

    [property: JsonPropertyName("EstaCancelado")]
    public bool? IsCanceled { get; set; }
}