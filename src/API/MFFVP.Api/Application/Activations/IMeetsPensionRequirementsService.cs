using Activations.Integrations.MeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements.CreateMeetsPensionRequirement;
using Activations.Integrations.MeetsPensionRequirements.UpdateMeetsPensionRequirement;
using Common.SharedKernel.Domain;
using MediatR;

namespace MFFVP.Api.Application.Activations;

public interface IMeetsPensionRequirementsService
{
    Task<Result<IReadOnlyCollection<MeetsPensionRequirementResponse>>> GetMeetsPensionRequirementsAsync(ISender sender);
    Task<Result<MeetsPensionRequirementResponse>> GetMeetsPensionRequirementAsync(int id, ISender sender);
    Task<Result> CreateMeetsPensionRequirementAsync(CreateMeetsPensionRequirementCommand request, ISender sender);
    Task<Result> UpdateMeetsPensionRequirementAsync(UpdateMeetsPensionRequirementCommand request, ISender sender);
    Task<Result> DeleteMeetsPensionRequirementAsync(int id, ISender sender);
}