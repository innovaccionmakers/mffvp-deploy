using Common.SharedKernel.Domain;
using MediatR;
using Associate.Integrations.PensionRequirements;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Associate.Integrations.PensionRequirements.GetPensionRequirements;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;

using MFFVP.Api.Application.Associate;

namespace MFFVP.Api.Infrastructure.Associate
{
    public sealed class PensionRequirementsService : IPensionRequirementsService
    {
        public async Task<Result<IReadOnlyCollection<PensionRequirementResponse>>> GetPensionRequirementsAsync(ISender sender)
        {
            return await sender.Send(new GetPensionRequirementsQuery());
        }

        public async Task<Result> CreatePensionRequirementAsync(CreatePensionRequirementCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdatePensionRequirementAsync(UpdatePensionRequirementCommand request, ISender sender)
        {
            return await sender.Send(request);
        }
    }
}