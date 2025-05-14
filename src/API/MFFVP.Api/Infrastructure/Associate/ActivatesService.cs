using Associate.Integrations.Activates;
using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.Activates.GetActivates;
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

    public async Task<Result<ActivateResponse>> CreateActivateAsync(CreateActivateCommand request, ISender sender)
    {
        return await sender.Send(request);
    }
}