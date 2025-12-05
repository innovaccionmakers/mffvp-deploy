using Accounting.Integrations.ConfigurationGenerals.CreateConfigurationGeneral;
using Accounting.Integrations.ConfigurationGenerals.DeleteConfigurationGeneral;
using Accounting.Integrations.ConfigurationGenerals.UpdateConfigurationGeneral;
using Accounting.Presentation.ConfigurationGenerals.CreateConfigurationGeneral;
using Accounting.Presentation.ConfigurationGenerals.DeleteConfigurationGeneral;
using Accounting.Presentation.ConfigurationGenerals.UpdateConfigurationGeneral;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class ConfigurationGeneralsExperienceMutations(ISender mediator) : IConfigurationGeneralsExperienceMutations
    {
        public async Task<GraphqlResult> CreateConfiguracionGeneralAsync(CreateConfigurationGeneralInput input, IValidator<CreateConfigurationGeneralInput> validator, CancellationToken cancellationToken = default)
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

                var command = new CreateConfigurationGeneralCommand(
                    input.PortfolioId,
                    input.AccountingCode,
                    input.CostCenter
                );

                var commandResult = await mediator.Send(command, cancellationToken);

                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }

                result.SetSuccess("Genial!, Se ha creado la configuración general exitosamente");
                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }

        public async Task<GraphqlResult> DeleteConfiguracionGeneralAsync(DeleteConfigurationGeneralInput input, IValidator<DeleteConfigurationGeneralInput> validator, CancellationToken cancellationToken = default)
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

                var command = new DeleteConfigurationGeneralCommand(
                    input.PortfolioId
                );

                var commandResult = await mediator.Send(command, cancellationToken);
                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }
                result.SetSuccess("Genial!, Se ha eliminado la configuración general exitosamente");

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }

        public async Task<GraphqlResult> UpdateConfiguracionGeneralAsync(UpdateConfigurationGeneralInput input, IValidator<UpdateConfigurationGeneralInput> validator, CancellationToken cancellationToken = default)
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

                var command = new UpdateConfigurationGeneralCommand(
                    input.PortfolioId,
                    input.AccountingCode,
                    input.CostCenter
                );

                var commandResult = await mediator.Send(command, cancellationToken);
                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }
                result.SetSuccess("Genial!, Se ha actualizado la configuración general exitosamente");

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
