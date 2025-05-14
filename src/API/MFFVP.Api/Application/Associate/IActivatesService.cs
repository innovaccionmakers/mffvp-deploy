using Associate.Integrations.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Common.SharedKernel.Domain;
using MediatR;

namespace MFFVP.Api.Application.Associate;

public interface IActivatesService
{
    Task<Result<IReadOnlyCollection<ActivateResponse>>> GetActivatesAsync(ISender sender);
    Task<Result<ActivateResponse>> CreateActivateAsync(CreateActivateCommand request, ISender sender);
}