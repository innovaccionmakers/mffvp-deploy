using Common.SharedKernel.Domain;
using MediatR;
using Operations.Integrations.Contributions;
using Operations.Integrations.Contributions.CreateContribution;

namespace MFFVP.Api.Application.Operations;

public interface IOperationsService
{
    Task<Result<ContributionResponse>> CreateContribution(CreateContributionCommand request, ISender sender);
}