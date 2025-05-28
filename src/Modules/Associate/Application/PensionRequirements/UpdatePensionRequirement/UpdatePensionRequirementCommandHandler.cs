using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.PensionRequirements;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
using Associate.Integrations.PensionRequirements;
using Associate.Application.Abstractions.Data;

namespace Associate.Application.PensionRequirements;
internal sealed class UpdatePensionRequirementCommandHandler{}
// internal sealed class UpdatePensionRequirementCommandHandler(
//     IPensionRequirementRepository pensionrequirementRepository,
//     IUnitOfWork unitOfWork)
//     : ICommandHandler<UpdatePensionRequirementCommand, PensionRequirementResponse>
// {
//     public async Task<Result<PensionRequirementResponse>> Handle(UpdatePensionRequirementCommand request, CancellationToken cancellationToken)
//     {
//         await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

//         var entity = await pensionrequirementRepository.GetAsync(request.PensionRequirementId, cancellationToken);
//         if (entity is null)
//         {
//             return Result.Failure<PensionRequirementResponse>(PensionRequirementErrors.NotFound(request.PensionRequirementId));
//         }

//         entity.UpdateDetails(
//             request.NewAffiliateId, 
//             request.NewStartDate, 
//             request.NewExpirationDate, 
//             request.NewCreationDate, 
//             request.NewStatus
//         );

//         await unitOfWork.SaveChangesAsync(cancellationToken);
//         await transaction.CommitAsync(cancellationToken);

//         return new PensionRequirementResponse(entity.PensionRequirementId, entity.AffiliateId, entity.StartDate, entity.ExpirationDate, entity.CreationDate, entity.Status);
//     }
// }