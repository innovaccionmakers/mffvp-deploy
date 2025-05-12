using Common.SharedKernel.Application.Messaging;

namespace Trusts.Integrations.InputInfos.DeleteInputInfo;

public sealed record DeleteInputInfoCommand(
    Guid InputInfoId
) : ICommand;