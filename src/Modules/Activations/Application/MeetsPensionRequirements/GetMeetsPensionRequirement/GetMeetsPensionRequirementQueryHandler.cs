using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Activations.Domain.MeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements.GetMeetsPensionRequirement;
using Activations.Integrations.MeetsPensionRequirements;

namespace Activations.Application.MeetsPensionRequirements.GetMeetsPensionRequirement;

internal sealed class GetMeetsPensionRequirementQueryHandler(
    IMeetsPensionRequirementRepository meetspensionrequirementRepository)
    : IQueryHandler<GetMeetsPensionRequirementQuery, MeetsPensionRequirementResponse>
{
    public async Task<Result<MeetsPensionRequirementResponse>> Handle(GetMeetsPensionRequirementQuery request, CancellationToken cancellationToken)
    {
        var meetspensionrequirement = await meetspensionrequirementRepository.GetAsync(request.MeetsPensionRequirementId, cancellationToken);
        if (meetspensionrequirement is null)
        {
            return Result.Failure<MeetsPensionRequirementResponse>(MeetsPensionRequirementErrors.NotFound(request.MeetsPensionRequirementId));
        }
        var response = new MeetsPensionRequirementResponse(
            meetspensionrequirement.MeetsPensionRequirementId,
            meetspensionrequirement.AffiliateId,
            meetspensionrequirement.StartDate,
            meetspensionrequirement.ExpirationDate,
            meetspensionrequirement.CreationDate,
            meetspensionrequirement.State
        );
        return response;
    }
}