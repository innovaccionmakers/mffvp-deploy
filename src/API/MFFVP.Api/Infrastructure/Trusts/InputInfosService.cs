using Common.SharedKernel.Domain;
using MediatR;
using MFFVP.Api.Application.Trusts;
using Trusts.Integrations.InputInfos;
using Trusts.Integrations.InputInfos.CreateInputInfo;
using Trusts.Integrations.InputInfos.DeleteInputInfo;
using Trusts.Integrations.InputInfos.GetInputInfo;
using Trusts.Integrations.InputInfos.GetInputInfos;
using Trusts.Integrations.InputInfos.UpdateInputInfo;

namespace MFFVP.Api.Infrastructure.Trusts;

public sealed class InputInfosService : IInputInfosService
{
    public async Task<Result<IReadOnlyCollection<InputInfoResponse>>> GetInputInfosAsync(ISender sender)
    {
        return await sender.Send(new GetInputInfosQuery());
    }

    public async Task<Result<InputInfoResponse>> GetInputInfoAsync(Guid id, ISender sender)
    {
        return await sender.Send(new GetInputInfoQuery(id));
    }

    public async Task<Result> CreateInputInfoAsync(CreateInputInfoCommand request, ISender sender)
    {
        return await sender.Send(request);
    }

    public async Task<Result> UpdateInputInfoAsync(UpdateInputInfoCommand request, ISender sender)
    {
        return await sender.Send(request);
    }

    public async Task<Result> DeleteInputInfoAsync(Guid id, ISender sender)
    {
        return await sender.Send(new DeleteInputInfoCommand(id));
    }
}