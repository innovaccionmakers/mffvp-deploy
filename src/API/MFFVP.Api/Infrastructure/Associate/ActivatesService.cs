using Associate.Integrations.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.Activates.GetActivate;
using Associate.Integrations.Activates.GetActivates;
using Associate.Integrations.Activates.UpdateActivate;
using Common.SharedKernel.Domain;
using MediatR;
using MFFVP.Api.Application.Associate;

namespace MFFVP.Api.Infrastructure.Associate;

public sealed class ActivatesService : IActivatesService
{
    public async Task<Result<IReadOnlyCollection<ActivateResponse>>> GetActivatesAsync(ISender sender)
    {
        return await sender.Send(new GetActivatesQuery());
    }

    public async Task<Result> CreateActivateAsync(CreateActivateCommand request, ISender sender)
    {
        return await sender.Send(request);
    }

    public async Task<Result> GetActivateAsync(long activateId, ISender sender)
    {
        return await sender.Send(new GetActivateQuery(activateId));
    }
    
    public async Task<Result> UpdateActivateAsync(UpdateActivateCommand request, ISender sender)
    {
        return await sender.Send(request);
    }
}