using System;

namespace Operations.IntegrationEvents.TrustOperations;

public sealed record GetTrustOperationsByPortfolioProcessDateAndTypeRequest(
    int PortfolioId,
    DateTime ProcessDate,
    long OperationTypeId);
