using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.Trusts.DeleteTrust;

public sealed record DeleteTrustCommand(
    long TrustId
) : ICommand;