using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.ClientOperations.GetClientOperation;
public sealed record GetClientOperationQuery(
    Guid ClientOperationId
) : IQuery<ClientOperationResponse>;