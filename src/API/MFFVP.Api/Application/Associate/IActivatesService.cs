using Associate.Integrations.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.Activates.UpdateActivate;
using Common.SharedKernel.Domain;
using MediatR;

namespace MFFVP.Api.Application.Associate;

public interface IActivatesService
{
    Task<Result<IReadOnlyCollection<ActivateResponse>>> GetActivatesAsync(ISender sender);
    Task<Result> CreateActivateAsync(CreateActivateCommand request, ISender sender);
    Task<Result> UpdateActivateAsync(UpdateActivateCommand request, ISender sender);
}