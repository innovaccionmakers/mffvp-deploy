using Accounting.Integrations.AccountingFees;
using Accounting.Integrations.AccountingReturns;
using Accounting.Integrations.AccountProcess;
using Accounting.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Accounting.Presentation.GraphQL
{
    public class AccountProcessExperienceMutations(
        IMediator mediator,
        ILogger<AccountProcessExperienceMutations> logger) : IAccountProcessExperienceMutations
    {
        public async Task<GraphqlResult<bool>> AccountingFeesProcessAsync(AccountingInput input, IValidator<AccountingInput> validator, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<bool>();

            try
            {
                var validationResult = await RequestValidator.Validate(input, validator);

                if (validationResult is not null)
                {
                    result.AddError(validationResult.Error);
                    return result;
                }

                var command = new AccountingFeesCommand(
                    input.PortfolioIds,
                    input.ProcessDate.ToUniversalTime()
                );

                var commandResult = await mediator.Send(command, cancellationToken);

                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }

                result.SetSuccess(commandResult.Value);

                return result;
            }
            catch(Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }

        public async Task<GraphqlResult<bool>> AccountingReturnsProcessAsync(AccountingInput input, IValidator<AccountingInput> validator, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<bool>();
            try
            {
                var validationResult = await RequestValidator.Validate(input, validator);

                if (validationResult is not null)
                {
                    result.AddError(validationResult.Error);
                    return result;
                }

                var command = new AccountingReturnsCommand(
                    input.PortfolioIds,
                    input.ProcessDate.ToUniversalTime()
                );

                var commandResult = await mediator.Send(command, cancellationToken);

                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }

                result.SetSuccess(commandResult.Value);
                return result;
            }
            catch(Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }

        public async Task<GraphqlResult<string>> AccountProcessAsync(AccountingInput input, IValidator<AccountingInput> validator, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<string>();

            try
            {
                var validationResult = await RequestValidator.Validate(input, validator);

                if (validationResult is not null)
                {
                    result.AddError(validationResult.Error);
                    return result;
                }

                var command = new AccountProcessCommand(
                    input.PortfolioIds,
                    input.ProcessDate
                );

                // Crear la tarea en segundo plano
                _ = Task.Run(async () =>
                {
                    try
                    {
                        logger.LogInformation("Procesando AccountProcessCommand en segundo plano."); 
                        var result = await mediator.Send(command, cancellationToken);

                        if (result.IsSuccess)
                        {
                            logger.LogInformation("Proceso AccountProcess completado exitosamente.");
                        }
                        else
                        {
                            logger.LogWarning("Proceso AccountProcess completado con errores. Error: {Error}", result.Error?.Description);
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error en proceso en segundo plano de AccountProcess.");
                    }
                }, cancellationToken);


                result.SetSuccess( string.Empty, "Se está generando la información del proceso contable. Sera notificado cuando finalice.");

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
