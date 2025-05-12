using System.Data.Common;
using Activations.Application.Abstractions.Data;
using Activations.Domain.Affiliates;
using Activations.Domain.MeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements;
using Activations.Integrations.MeetsPensionRequirements.CreateMeetsPensionRequirement;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Activations.Application.MeetsPensionRequirements.CreateMeetsPensionRequirement
{
    internal sealed class CreateMeetsPensionRequirementCommandHandler(
        IMeetsPensionRequirementRepository meetspensionrequirementRepository,
        IUnitOfWork unitOfWork)
        : ICommandHandler<CreateMeetsPensionRequirementCommand, MeetsPensionRequirementResponse>
    {
        public async Task<Result<MeetsPensionRequirementResponse>> Handle(CreateMeetsPensionRequirementCommand request, CancellationToken cancellationToken)
        {
            await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

            var result = MeetsPensionRequirement.Create(
                request.StartDate,
                request.ExpirationDate,
                request.CreationDate,
                request.State,
                request.AffiliateId
            );

            if (result.IsFailure)
            {
                return Result.Failure<MeetsPensionRequirementResponse>(result.Error);
            }

            var meetspensionrequirement = result.Value;
            
            meetspensionrequirementRepository.Insert(meetspensionrequirement);

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new MeetsPensionRequirementResponse(
                meetspensionrequirement.MeetsPensionRequirementId,
                meetspensionrequirement.AffiliateId,
                meetspensionrequirement.StartDate,
                meetspensionrequirement.ExpirationDate,
                meetspensionrequirement.CreationDate,
                meetspensionrequirement.State
            );
        }
    }
}