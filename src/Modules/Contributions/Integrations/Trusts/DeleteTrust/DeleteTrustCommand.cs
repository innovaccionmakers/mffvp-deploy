using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.Trusts.DeleteTrust;
public sealed record DeleteTrustCommand(
    Guid TrustId
) : ICommand;