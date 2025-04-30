using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.ClientOperations.DeleteClientOperation;
public sealed record DeleteClientOperationCommand(
    Guid ClientOperationId
) : ICommand;