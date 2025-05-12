using Common.SharedKernel.Domain;
using MediatR;
using Trusts.Integrations.FullContribution;

namespace MFFVP.Api.Application.Trusts;

public interface IFullContributionService
{
    Task<Result<FullContributionResponse>> CreateFullContributionAsync(
        CreateFullContributionCommand request,
        ISender sender);
}