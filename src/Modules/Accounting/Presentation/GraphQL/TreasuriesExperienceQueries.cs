using Accounting.Integrations.Treasury.GetTreasuries;
using Accounting.Presentation.DTOs;
using Accounting.Presentation.GraphQL.Inputs.TreasuriesInput;
using Common.SharedKernel.Core.Primitives;
using Common.SharedKernel.Presentation.Results;
using MediatR;
using Microsoft.IdentityModel.Tokens;

namespace Accounting.Presentation.GraphQL
{
    public class TreasuriesExperienceQueries(
        ISender mediator) : ITreasuriesExperienceQueries
    {
        public async Task<GraphqlResult<TreasuryDto>> GetTreasuriesAsync(GetTreasuryInput input, CancellationToken cancellationToken = default)
        {
            var result = new GraphqlResult<TreasuryDto>();
            try
            {
                if (input.BankAccount.IsNullOrEmpty())
                {
                    result.AddError(new Error("General.Validation", "El campo cuenta bancaria es obligatorio y no puede estar vacío.", ErrorType.Failure));
                    return result;
                }

                var response = await mediator.Send(new GetTreasuryQuery(input.PortfolioId, input.BankAccount), cancellationToken);

                if (!response.IsSuccess || response.Value == null)
                {
                    result.AddError(response.Error);
                    return result;
                }

                var getTreasury = response.Value;

                var Treasury = new TreasuryDto(
                    getTreasury.BankAccount,
                    getTreasury.DebitAccount,
                    getTreasury.CreditAccount
                );

                result.Data = Treasury;

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
