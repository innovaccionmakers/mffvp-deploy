using Associate.Integrations.Activates.CreateActivate;
using Associate.Integrations.Activates.UpdateActivate;
using Associate.Integrations.PensionRequirements.CreatePensionRequirement;
using Associate.Integrations.PensionRequirements.UpdatePensionRequirement;
using Associate.Presentation.GraphQL.Inputs;

using Common.SharedKernel.Core.Primitives;
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
                input.StartDateReqPen?.ToDateTime(TimeOnly.MinValue).ToUniversalTime(),
                input.EndDateReqPen?.ToDateTime(TimeOnly.MinValue).ToUniversalTime()
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

    public async Task<GraphqlMutationResult> RegisterPensionRequirementsAsync(CreatePensionRequirementInput input, IValidator<CreatePensionRequirementInput> validator, CancellationToken cancellationToken)
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

            var command = new CreatePensionRequirementCommand(
                input.DocumentType,
                input.Identification,
                input.StartDateReqPen?.ToDateTime(TimeOnly.MinValue).ToUniversalTime(),
                input.EndDateReqPen?.ToDateTime(TimeOnly.MinValue).ToUniversalTime()
            );

            var commandResult = await mediator.Send(command, cancellationToken);

            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }

            result.SetSuccess("Genial!, Se ha creado el Certificado de Requisitos de Pensión");
            return result;
        }
        catch (Exception ex)
        {
            result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
            return result;
        }
    }

    public async Task<GraphqlMutationResult> UpdateActivateAsync(UpdateActivateInput input, IValidator<UpdateActivateInput> validator, CancellationToken cancellationToken)
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

            var command = new UpdateActivateCommand(
                input.DocumentType,
                input.Identification,
                input.Pensioner
            );

            var commandResult = await mediator.Send(command, cancellationToken);
            if (!commandResult.IsSuccess)
            {
                result.AddError(commandResult.Error);
                return result;
            }
            result.SetSuccess("Genial!, Se ha actualizado la Condición de Pensión del Afiliado");

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
            var validationResult = await RequestValidator.Validate(input, validator);
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
