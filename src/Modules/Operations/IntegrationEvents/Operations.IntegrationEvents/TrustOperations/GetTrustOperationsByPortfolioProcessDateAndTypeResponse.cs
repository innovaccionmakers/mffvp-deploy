using Operations.Integrations.TrustOperations;
using System.Collections.Generic;

namespace Operations.IntegrationEvents.TrustOperations;

public sealed record GetTrustOperationsByPortfolioProcessDateAndTypeResponse(
    bool Succeeded,
    string? Code,
    string? Message,
    IReadOnlyCollection<TrustOperationResponse> Operations);
