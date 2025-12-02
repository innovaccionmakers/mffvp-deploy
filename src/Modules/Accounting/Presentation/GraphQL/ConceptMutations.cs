using Accounting.Integrations.Concept.CreateConcept;
using Accounting.Integrations.Concept.DeleteConcept;
using Accounting.Integrations.Concept.UpdateConcept;
using Accounting.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class ConceptMutations(
        ISender mediator) : IConceptMutations
    {
        public async Task<GraphqlResult> CreateConceptAsync(CreateConceptInput input, IValidator<CreateConceptInput> validator, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult();
            try
            {
                var validationResult = await RequestValidator.Validate(input, validator);

                if (validationResult is not null)
                {
                    result.AddError(validationResult.Error);
                    return result;
                }

                var command = new CreateConceptCommand(
                    input.PortfolioId,
                    input.Name,
                    input.DebitAccount,
                    input.CreditAccount
                );

                var commandResult = await mediator.Send(command, cancellationToken);

                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }

                result.SetSuccess("Genial!, Se ha creado el concepto exitosamente");
                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }

        public async Task<GraphqlResult> DeleteConceptAsync(DeleteConceptInput input, IValidator<DeleteConceptInput> validator, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult();
            try
            {
                var validationResult = await RequestValidator.Validate(input, validator);
                if (validationResult is not null)
                {
                    result.AddError(validationResult.Error);
                    return result;
                }

                var command = new DeleteConceptCommand(
                    input.ConceptId
                );

                var commandResult = await mediator.Send(command, cancellationToken);
                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }
                result.SetSuccess("Genial!, Se ha eliminado el concepto exitosamente");

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }

        public async Task<GraphqlResult> UpdateConceptAsync(UpdateConceptInput input, IValidator<UpdateConceptInput> validator, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult();
            try
            {
                var validationResult = await RequestValidator.Validate(input, validator);
                if (validationResult is not null)
                {
                    result.AddError(validationResult.Error);
                    return result;
                }

                var command = new UpdateConceptCommand(
                    input.ConceptId,
                    input.DebitAccount,
                    input.CreditAccount
                );

                var commandResult = await mediator.Send(command, cancellationToken);
                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }
                result.SetSuccess("Genial!, Se ha actualizado el concepto exitosamente");

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }
    }
}

