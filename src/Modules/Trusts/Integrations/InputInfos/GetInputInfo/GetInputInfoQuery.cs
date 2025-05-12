using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.InputInfos.GetInputInfo;

public sealed record GetInputInfoQuery(
    Guid InputInfoId
) : IQuery<InputInfoResponse>;