using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Contributions.Integrations.TrustOperations.GetTrustOperations;
public sealed record GetTrustOperationsQuery() : IQuery<IReadOnlyCollection<TrustOperationResponse>>;