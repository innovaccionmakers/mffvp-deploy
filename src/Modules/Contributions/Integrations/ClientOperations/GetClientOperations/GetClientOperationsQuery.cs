using Common.SharedKernel.Application.Messaging;
using System;
using System.Collections.Generic;

namespace Contributions.Integrations.ClientOperations.GetClientOperations;
public sealed record GetClientOperationsQuery() : IQuery<IReadOnlyCollection<ClientOperationResponse>>;