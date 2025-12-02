using Accounting.Application.Abstractions.Data;
using Accounting.Domain.Concepts;
using Accounting.Integrations.Concept.CreateConcept;
using Accounting.Integrations.Concept.GetConceptsByPortfolioIds;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Domain;
using Microsoft.Extensions.Logging;

namespace Accounting.Application.Concept.CreateConcept
{
    internal class CreateConceptCommandHandler(
        IConceptsRepository conceptsRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateConceptCommandHandler> logger) : ICommandHandler<CreateConceptCommand>
    {
        public async Task<Result> Handle(CreateConceptCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = Domain.Concepts.Concept.Create(
                    request.PortfolioId,
                    request.Name,
                    request.DebitAccount,
                    request.CreditAccount
                );

                conceptsRepository.Insert(result.Value);
                await unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success();
            }
            catch (Exception ex)
            {
                logger.LogError("Error al crear el concepto: Error: {Message}", ex.Message);
                return Result.Failure(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            }
        }
    }
}

