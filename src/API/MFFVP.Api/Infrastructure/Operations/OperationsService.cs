using Common.SharedKernel.Domain;
using MediatR;
using MFFVP.Api.Application.Operations;
using Operations.Integrations.Contributions;
using Operations.Integrations.Contributions.CreateContribution;

namespace MFFVP.Api.Infrastructure.Operations;

public class OperationsService : IOperationsService
{
    public async Task<Result<ContributionResponse>> CreateContribution(CreateContributionCommand request, ISender sender)
    {
        return await sender.Send(request);
    }
}