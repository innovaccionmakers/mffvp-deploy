using Accounting.Application.Abstractions.Data;
using Accounting.Domain.Concepts;
using Accounting.Integrations.Concept.GetConceptsByPortfolioIds;
using Accounting.Integrations.Concept.UpdateConcept;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Concept.UpdateConcept
{
    internal class UpdateConceptCommandHandler(
        IConceptsRepository conceptsRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateConceptCommandHandler> logger) : ICommandHandler<UpdateConceptCommand>
    {
        public async Task<Result> Handle(UpdateConceptCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var concept = await conceptsRepository.GetByPortfolioIdAndNameAsync(request.PortfolioId, request.Name, cancellationToken);

                if (concept is null)
                    return Result.Failure(Error.NotFound("0", "No hay concepto."));

                concept.UpdateDetails(
                    request.PortfolioId,
                    request.Name,
                    request.DebitAccount,
                    request.CreditAccount);

                conceptsRepository.Update(concept);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al actualizar el concepto. Error: {Message}", ex.Message);
                return Result.Failure<GetConceptsByPortfolioIdsResponse>(Error.NotFound("0", "No se pudo actualizar el concepto."));
            }
        }
    }
}

