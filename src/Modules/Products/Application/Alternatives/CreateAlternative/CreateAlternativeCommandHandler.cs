using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Application.Abstractions.Data;
using Products.Domain.Alternatives;
using Products.Integrations.Alternatives;
using Products.Integrations.Alternatives.CreateAlternative;

namespace Products.Application.Alternatives.CreateAlternative;

internal sealed class CreateAlternativeCommandHandler(
    IAlternativeRepository alternativeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateAlternativeCommand, AlternativeResponse>
{
    public async Task<Result<AlternativeResponse>> Handle(CreateAlternativeCommand request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);


        var result = Alternative.Create(
            request.AlternativeTypeId,
            request.Name,
            request.Status,
            request.Description
        );

        if (result.IsFailure) return Result.Failure<AlternativeResponse>(result.Error);

        var alternative = result.Value;

        alternativeRepository.Insert(alternative);

        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new AlternativeResponse(
            alternative.AlternativeId,
            alternative.AlternativeTypeId,
            alternative.Name,
            alternative.Status,
            alternative.Description
        );
    }
}