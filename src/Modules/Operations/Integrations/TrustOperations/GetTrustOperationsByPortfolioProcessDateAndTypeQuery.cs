using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Operations.Integrations.TrustOperations;

public sealed record GetTrustOperationsByPortfolioProcessDateAndTypeQuery(
    int PortfolioId,
    DateTime ProcessDate,
    long OperationTypeId) : IQuery<IReadOnlyCollection<TrustOperationResponse>>;
