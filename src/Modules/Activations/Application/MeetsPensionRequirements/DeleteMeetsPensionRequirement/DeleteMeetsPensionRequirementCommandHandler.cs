using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Activations.Domain.MeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements.DeleteMeetsPensionRequirement;
using Activations.Application.Abstractions.Data;

namespace Activations.Application.MeetsPensionRequirements.DeleteMeetsPensionRequirement;

internal sealed class DeleteMeetsPensionRequirementCommandHandler(
    IMeetsPensionRequirementRepository meetspensionrequirementRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteMeetsPensionRequirementCommand>
{
    public async Task<Result> Handle(DeleteMeetsPensionRequirementCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var meetspensionrequirement = await meetspensionrequirementRepository.GetAsync(request.MeetsPensionRequirementId, cancellationToken);
        if (meetspensionrequirement is null)
        {
            return Result.Failure(MeetsPensionRequirementErrors.NotFound(request.MeetsPensionRequirementId));
        }
        
        meetspensionrequirementRepository.Delete(meetspensionrequirement);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}