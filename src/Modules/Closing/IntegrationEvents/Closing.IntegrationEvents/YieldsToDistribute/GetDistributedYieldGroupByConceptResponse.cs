using Closing.Integrations.YieldsToDistribute;

namespace Closing.IntegrationEvents.YieldsToDistribute;

public sealed record GetDistributedYieldGroupByConceptResponse
(
    bool IsValid,
    IReadOnlyCollection<DistributedYieldGroupResponse> DistributedYieldGroups,
    string? Code,
    string? Message
);
