using Activations.Application.Abstractions.Data;
using Activations.Domain.MeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements.UpdateMeetsPensionRequirement;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Activations.Application.MeetsPensionRequirements;

internal sealed class UpdateMeetsPensionRequirementCommandHandler(
    IMeetsPensionRequirementRepository meetspensionrequirementRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateMeetsPensionRequirementCommand, MeetsPensionRequirementResponse>
{
    public async Task<Result<MeetsPensionRequirementResponse>> Handle(UpdateMeetsPensionRequirementCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity =
            await meetspensionrequirementRepository.GetAsync(request.MeetsPensionRequirementId, cancellationToken);
        if (entity is null)
            return Result.Failure<MeetsPensionRequirementResponse>(
                MeetsPensionRequirementErrors.NotFound(request.MeetsPensionRequirementId));

        entity.UpdateDetails(
            request.NewAffiliateId,
            request.NewStartDate,
            request.NewExpirationDate,
            request.NewCreationDate,
            request.NewState
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new MeetsPensionRequirementResponse(entity.MeetsPensionRequirementId, entity.AffiliateId,
            entity.StartDate, entity.ExpirationDate, entity.CreationDate, entity.State);
    }
}