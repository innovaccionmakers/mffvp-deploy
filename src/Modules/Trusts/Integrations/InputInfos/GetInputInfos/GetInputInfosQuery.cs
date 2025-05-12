using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.InputInfos.GetInputInfos;

public sealed record GetInputInfosQuery : IQuery<IReadOnlyCollection<InputInfoResponse>>;