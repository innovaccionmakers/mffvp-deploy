
using Common.SharedKernel.Domain;
using System.Text.Json;

namespace Closing.Domain.YieldsToDistribute;

public sealed class YieldToDistribute : Entity
{
    public long YieldToDistributeId { get; private set; }
    public long TrustId { get; private set; }
    public int PortfolioId { get; private set; }
    public DateTime ClosingDate { get; private set; }

    public DateTime ApplicationDate { get; private set; }
    public decimal Participation { get; private set; }

    public decimal YieldAmount { get; private set; }

    public JsonDocument Concept { get; private set; } = null!;

    public DateTime ProcessDate { get; private set; }
}
