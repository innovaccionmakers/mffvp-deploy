using Common.SharedKernel.Domain;
using MediatR;

using Activations.Integrations.MeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements.CreateMeetsPensionRequirement;
using Activations.Integrations.MeetsPensionRequirements.DeleteMeetsPensionRequirement;
using Activations.Integrations.MeetsPensionRequirements.GetMeetsPensionRequirement;
using Activations.Integrations.MeetsPensionRequirements.GetMeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements.UpdateMeetsPensionRequirement;

using MFFVP.Api.Application.Activations;

namespace MFFVP.Api.Infrastructure.Activations
{
    public sealed class MeetsPensionRequirementsService : IMeetsPensionRequirementsService
    {
        public async Task<Result<IReadOnlyCollection<MeetsPensionRequirementResponse>>> GetMeetsPensionRequirementsAsync(ISender sender)
        {
            return await sender.Send(new GetMeetsPensionRequirementsQuery());
        }

        public async Task<Result<MeetsPensionRequirementResponse>> GetMeetsPensionRequirementAsync(int id, ISender sender)
        {
            return await sender.Send(new GetMeetsPensionRequirementQuery(id));
        }

        public async Task<Result> CreateMeetsPensionRequirementAsync(CreateMeetsPensionRequirementCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> UpdateMeetsPensionRequirementAsync(UpdateMeetsPensionRequirementCommand request, ISender sender)
        {
            return await sender.Send(request);
        }

        public async Task<Result> DeleteMeetsPensionRequirementAsync(int id, ISender sender)
        {
            return await sender.Send(new DeleteMeetsPensionRequirementCommand(id));
        }
    }
}