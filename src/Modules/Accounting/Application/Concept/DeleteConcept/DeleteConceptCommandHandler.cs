using Accounting.Application.Abstractions.Data;
using Accounting.Domain.Concepts;
using Accounting.Integrations.Concept.DeleteConcept;
using Accounting.Integrations.Concept.GetConceptsByPortfolioIds;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Concept.DeleteConcept
{
    internal class DeleteConceptCommandHandler(
        IConceptsRepository conceptsRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteConceptCommandHandler> logger) : ICommandHandler<DeleteConceptCommand>
    {
        public async Task<Result> Handle(DeleteConceptCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await using var tx = await unitOfWork.BeginTransactionAsync(cancellationToken);
                var concept = await conceptsRepository.GetByPortfolioIdAndNameAsync(request.PortfolioId, request.Name, cancellationToken);

                if (concept is null)
                    return Result.Failure(Error.NullValue);

                conceptsRepository.Delete(concept);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await tx.CommitAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al eliminar el concepto. Error: {Message}", ex.Message);
                return Result.Failure<GetConceptsByPortfolioIdsResponse>(Error.NotFound("0", "No se pudo eliminar el concepto."));
            }
        }
    }
}

