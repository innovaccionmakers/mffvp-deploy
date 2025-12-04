using Accounting.Domain.Concepts;
using Accounting.Integrations.Concept.GetConcepts;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Concept.GetConcepts
{
    internal class GetConceptsQueryHandler(
        IConceptsRepository conceptsRepository,
        ILogger<GetConceptsQueryHandler> logger) : IQueryHandler<GetConceptsQuery, IReadOnlyCollection<GetConceptsResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetConceptsResponse>>> Handle(GetConceptsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var concepts = await conceptsRepository.GetAllAsync(cancellationToken);

                var response = concepts.Select(x => new GetConceptsResponse(
                    x.ConceptId,
                    x.PortfolioId,
                    x.Name,
                    x.DebitAccount,
                    x.CreditAccount
                )).ToList();

                return Result.Success<IReadOnlyCollection<GetConceptsResponse>>(response);
            }
            catch (Exception ex)
            {
                logger.LogError("Error al obtener los conceptos. Error: {Message}", ex.Message);
                throw new InvalidOperationException("No se pudieron obtener los conceptos.", ex);
            }
        }
    }
}

