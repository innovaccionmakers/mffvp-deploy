using Common.SharedKernel.Application.Messaging;
using System;

namespace Contributions.Integrations.Trusts.GetTrust;
public sealed record GetTrustQuery(
    Guid TrustId
) : IQuery<TrustResponse>;