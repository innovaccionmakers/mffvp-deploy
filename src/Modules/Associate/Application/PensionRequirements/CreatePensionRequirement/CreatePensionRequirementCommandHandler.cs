using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Associate.Domain.PensionRequirements;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Associate.Integrations.PensionRequirements;
using Associate.Application.Abstractions.Data;
using Associate.Domain.Activates;
namespace Associate.Application.PensionRequirements.CreatePensionRequirement

{
    internal sealed class CreatePensionRequirementCommandHandler{}

    // internal sealed class CreatePensionRequirementCommandHandler(
    //     IActivateRepository activateRepository,
    //     IPensionRequirementRepository pensionrequirementRepository,
    //     IUnitOfWork unitOfWork)
    //     : ICommandHandler<CreatePensionRequirementCommand, PensionRequirementResponse>
    // {
    //     public async Task<Result<PensionRequirementResponse>> Handle(CreatePensionRequirementCommand request, CancellationToken cancellationToken)
    //     {
    //         await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

    //         var activate = await activateRepository.GetAsync(request.ActivateId, cancellationToken);

    //         if (activate is null)
    //             return Result.Failure<PensionRequirementResponse>(ActivateErrors.NotFound(request.ActivateId));


    //         var result = PensionRequirement.Create(
    //             request.StartDate,
    //             request.ExpirationDate,
    //             request.CreationDate,
    //             request.Status,
    //             activate
    //         );

    //         if (result.IsFailure)
    //         {
    //             return Result.Failure<PensionRequirementResponse>(result.Error);
    //         }

    //         var pensionrequirement = result.Value;

    //         pensionrequirementRepository.Insert(pensionrequirement);

    //         await unitOfWork.SaveChangesAsync(cancellationToken);
    //         await transaction.CommitAsync(cancellationToken);

    //         return new PensionRequirementResponse(
    //             pensionrequirement.PensionRequirementId,
    //             pensionrequirement.AffiliateId,
    //             pensionrequirement.StartDate,
    //             pensionrequirement.ExpirationDate,
    //             pensionrequirement.CreationDate,
    //             pensionrequirement.Status
    //         );
    //     }
    // }
}