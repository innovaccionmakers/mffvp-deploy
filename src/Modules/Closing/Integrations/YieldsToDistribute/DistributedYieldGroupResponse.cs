using System.Text.Json;

namespace Closing.Integrations.YieldsToDistribute;

public sealed record DistributedYieldGroupResponse
(
    DateTime ClosinDate,
    int PortofolioId,
    JsonDocument Concept,
    decimal TotalYieldAmount
);

