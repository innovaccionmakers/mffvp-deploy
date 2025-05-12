using Activations.Application.Abstractions.Data;
using Activations.Domain.Affiliates;
using Activations.Integrations.Affiliates;
using Activations.Integrations.Affiliates.UpdateAffiliate;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Activations.Application.Affiliates;

internal sealed class UpdateAffiliateCommandHandler(
    IAffiliateRepository affiliateRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAffiliateCommand, AffiliateResponse>
{
    public async Task<Result<AffiliateResponse>> Handle(UpdateAffiliateCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await affiliateRepository.GetAsync(request.AffiliateId, cancellationToken);
        if (entity is null) return Result.Failure<AffiliateResponse>(AffiliateErrors.NotFound(request.AffiliateId));

        entity.UpdateDetails(
            request.NewIdentificationType,
            request.NewIdentification,
            request.NewPensioner,
            request.NewMeetsRequirements,
            request.NewActivationDate
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AffiliateResponse(entity.AffiliateId, entity.IdentificationType, entity.Identification,
            entity.Pensioner, entity.MeetsRequirements, entity.ActivationDate);
    }
}