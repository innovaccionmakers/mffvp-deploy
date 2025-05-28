using Common.SharedKernel.Domain;
using MediatR;

using Associate.Integrations.PensionRequirements;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;

namespace MFFVP.Api.Application.Associate
{
    public interface IPensionRequirementsService
    {
        Task<Result<IReadOnlyCollection<PensionRequirementResponse>>> GetPensionRequirementsAsync(ISender sender);
        Task<Result> CreatePensionRequirementAsync(CreatePensionRequirementCommand request, ISender sender);
        Task<Result> UpdatePensionRequirementAsync(UpdatePensionRequirementCommand request, ISender sender);
    }
}