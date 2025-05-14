using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Alternatives;
using Products.Integrations.Alternatives.UpdateAlternative;
using Products.Integrations.Alternatives;
using Products.Application.Abstractions.Data;

namespace Products.Application.Alternatives;
internal sealed class UpdateAlternativeCommandHandler(
    IAlternativeRepository alternativeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateAlternativeCommand, AlternativeResponse>
{
    public async Task<Result<AlternativeResponse>> Handle(UpdateAlternativeCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var entity = await alternativeRepository.GetAsync(request.AlternativeId, cancellationToken);
        if (entity is null)
        {
            return Result.Failure<AlternativeResponse>(AlternativeErrors.NotFound(request.AlternativeId));
        }

        entity.UpdateDetails(
            request.NewAlternativeTypeId, 
            request.NewName, 
            request.NewStatus, 
            request.NewDescription
        );

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AlternativeResponse(entity.AlternativeId, entity.AlternativeTypeId, entity.Name, entity.Status, entity.Description);
    }
}