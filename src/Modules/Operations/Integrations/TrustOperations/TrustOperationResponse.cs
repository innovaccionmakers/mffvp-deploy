using System;

namespace Operations.Integrations.TrustOperations;

public sealed record TrustOperationResponse(
    int PortfolioId,
    DateTime ProcessDate,
    TrustOperationTypeSummary OperationType,
    decimal Value);

public sealed record TrustOperationTypeSummary(long Id, string Name);
