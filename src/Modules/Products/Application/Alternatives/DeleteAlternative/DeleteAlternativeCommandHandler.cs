using System.Data.Common;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.Alternatives;
using Products.Integrations.Alternatives.DeleteAlternative;
using Products.Application.Abstractions.Data;

namespace Products.Application.Alternatives.DeleteAlternative;

internal sealed class DeleteAlternativeCommandHandler(
    IAlternativeRepository alternativeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<DeleteAlternativeCommand>
{
    public async Task<Result> Handle(DeleteAlternativeCommand request, CancellationToken cancellationToken)
    {
        await using DbTransaction transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        var alternative = await alternativeRepository.GetAsync(request.AlternativeId, cancellationToken);
        if (alternative is null)
        {
            return Result.Failure(AlternativeErrors.NotFound(request.AlternativeId));
        }
        
        alternativeRepository.Delete(alternative);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return Result.Success();
    }
}