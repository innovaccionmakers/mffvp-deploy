using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.Trusts.GetTrusts;

public sealed record GetTrustsQuery : IQuery<IReadOnlyCollection<TrustResponse>>;