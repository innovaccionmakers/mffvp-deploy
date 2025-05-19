using Common.SharedKernel.Application.Messaging;
using System;

namespace Operations.Integrations.ClientOperations.GetClientOperation;

public sealed record GetClientOperationQuery(
    long ClientOperationId
) : IQuery<ClientOperationResponse>;