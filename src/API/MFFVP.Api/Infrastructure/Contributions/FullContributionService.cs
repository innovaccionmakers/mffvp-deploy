using Common.SharedKernel.Domain;
using Contributions.Integrations.FullContribution;
using MediatR;
using MFFVP.Api.Application.Contributions;

namespace MFFVP.Api.Infrastructure.Contributions
{
    public sealed class FullContributionService : IFullContributionService
    {
        public async Task<Result<FullContributionResponse>> CreateFullContributionAsync(
            CreateFullContributionCommand request,
            ISender sender)
        {
            return await sender.Send(request);
        }
    }
}
