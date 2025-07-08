using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
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

    public async Task<GraphqlMutationResult> UpdatePensionRequirementsAsync(UpdatePensionRequirementInput input, IValidator<UpdatePensionRequirementInput> validator, CancellationToken cancellationToken)
    {
        var result = new GraphqlMutationResult();

        try
        {
            var validationResult = RequestValidator.Validate(input, validator).GetAwaiter().GetResult();
            if (validationResult is not null)
            {
                result.AddError(validationResult.Error);
                return result;
            }
            var command = new UpdatePensionRequirementCommand(
                input.DocumentType,
                input.Identification,
                input.PensionRequirementId,
                input.Status
            );
            var commandResult = await mediator.Send(command, cancellationToken);
            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }
            result.SetSuccess("Genial!, Se ha Inactivado el Certificado de Requisitos de Pensión");
            return result;

        } catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }
}
