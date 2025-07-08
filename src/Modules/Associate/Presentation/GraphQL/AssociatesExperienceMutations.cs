using Associate.Integrations.Activates.CreateActivate;
using Associate.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Domain;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using MediatR;

namespace Associate.Presentation.GraphQL;

public class AssociatesExperienceMutations(IMediator mediator) : IAssociatesExperienceMutations
{
    public async Task<GraphqlMutationResult> RegisterActivateAsync(CreateActivateInput input, IValidator<CreateActivateInput> validator, CancellationToken cancellationToken)
    {
        var result = new GraphqlMutationResult();
        try
        {
            var validationResult = await RequestValidator.Validate(input, validator);

            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }

            var command = new CreateActivateCommand(
                input.DocumentType,
                input.Identification,
                input.Pensioner,
                input.MeetsPensionRequirements,
                input.StartDateReqPen,
                input.EndDateReqPen
            );

            var commandResult = await mediator.Send(command, cancellationToken);
            
            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            result.SetSuccess("Genial!, Se ha activado el Afiliado");
            return result;
        }
        catch (Exception ex)
        {
           result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
           return result;
        }
    }
}
