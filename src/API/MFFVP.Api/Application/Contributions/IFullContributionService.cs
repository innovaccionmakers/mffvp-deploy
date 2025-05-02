using Common.SharedKernel.Domain;
using Contributions.Integrations.FullContribution;
using MediatR;

namespace MFFVP.Api.Application.Contributions
{
    public interface IFullContributionService
    {
        Task<Result<FullContributionResponse>> CreateFullContributionAsync(
            CreateFullContributionCommand request,
            ISender sender);
    }
}
