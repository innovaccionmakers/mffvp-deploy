using Common.SharedKernel.Domain;
using MediatR;
using MFFVP.Api.Application.Trusts;
using Trusts.Integrations.FullContribution;

namespace MFFVP.Api.Infrastructure.Trusts;

public sealed class FullContributionService : IFullContributionService
{
    public async Task<Result<FullContributionResponse>> CreateFullContributionAsync(
        CreateFullContributionCommand request,
        ISender sender)
    {
        return await sender.Send(request);
    }
}