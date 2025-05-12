using Activations.Domain.MeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements.GetMeetsPensionRequirements;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Activations.Application.MeetsPensionRequirements.GetMeetsPensionRequirements;

internal sealed class GetMeetsPensionRequirementsQueryHandler(
    IMeetsPensionRequirementRepository meetspensionrequirementRepository)
    : IQueryHandler<GetMeetsPensionRequirementsQuery, IReadOnlyCollection<MeetsPensionRequirementResponse>>
{
    public async Task<Result<IReadOnlyCollection<MeetsPensionRequirementResponse>>> Handle(
        GetMeetsPensionRequirementsQuery request, CancellationToken cancellationToken)
    {
        var entities = await meetspensionrequirementRepository.GetAllAsync(cancellationToken);

        var response = entities
            .Select(e => new MeetsPensionRequirementResponse(
                e.MeetsPensionRequirementId,
                e.AffiliateId,
                e.StartDate,
                e.ExpirationDate,
                e.CreationDate,
                e.State))
            .ToList();

        return Result.Success<IReadOnlyCollection<MeetsPensionRequirementResponse>>(response);
    }
}