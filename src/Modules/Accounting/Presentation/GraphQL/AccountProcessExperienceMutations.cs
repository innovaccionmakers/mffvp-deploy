﻿using Accounting.Integrations.AccountingFees;
using Accounting.Integrations.AccountingReturns;
using Accounting.Integrations.AccountProcess;
using Accounting.Presentation.GraphQL.Inputs;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Filters;
using Common.SharedKernel.Presentation.Results;
using FluentValidation;
using MediatR;

namespace Accounting.Presentation.GraphQL
{
    public class AccountProcessExperienceMutations(IMediator mediator) : IAccountProcessExperienceMutations
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

                var commandResult = await mediator.Send(command, cancellationToken);

                if (!commandResult.IsSuccess)
                {
                    result.AddError(commandResult.Error);
                    return result;
                }

                var valueCommand = commandResult.Value;

                result.SetSuccess(new string(valueCommand));

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
