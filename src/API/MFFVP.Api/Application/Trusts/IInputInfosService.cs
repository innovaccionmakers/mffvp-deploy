using Common.SharedKernel.Domain;
using MediatR;
using Trusts.Integrations.InputInfos;
using Trusts.Integrations.InputInfos.CreateInputInfo;
using Trusts.Integrations.InputInfos.UpdateInputInfo;

namespace MFFVP.Api.Application.Trusts;

public interface IInputInfosService
{
    Task<Result<IReadOnlyCollection<InputInfoResponse>>> GetInputInfosAsync(ISender sender);
    Task<Result<InputInfoResponse>> GetInputInfoAsync(Guid id, ISender sender);
    Task<Result> CreateInputInfoAsync(CreateInputInfoCommand request, ISender sender);
    Task<Result> UpdateInputInfoAsync(UpdateInputInfoCommand request, ISender sender);
    Task<Result> DeleteInputInfoAsync(Guid id, ISender sender);
}