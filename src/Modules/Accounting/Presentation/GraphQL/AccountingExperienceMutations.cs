using Accounting.Integrations.AccountProcess;
using Accounting.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class AccountingExperienceMutations(IMediator mediator) : IAccountingExperienceMutations
    {       
        public async Task<GraphqlResult<AccountProcessResult>> AccountProcessAsync(AccountingInput input, IValidator<AccountingInput> validator, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<AccountProcessResult>();
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

                var commandResult = await mediator.Send(command, cancellationToken);

                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }

                result.SetSuccess(commandResult.Value, "Hemos recibido tu solicitud para el Informe de Contabilidad. Te mantendremos informado sobre el progreso a través del Centro de notificaciones y tu correo electrónico.");

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
