using Accounting.Integrations.Treasuries.GetTreasuries;
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
        public async Task<GraphqlResult<IReadOnlyCollection<TreasuryDto>>> GetTreasuriesAsync(GetTreasuryInput input, CancellationToken cancellationToken = default)
        {
            if(input.BankAccount.IsNullOrEmpty() && input.PortfolioId == null)
                return await GetTreasuries(cancellationToken);

            return await GetTreasury(input.PortfolioId ?? default, input.BankAccount, cancellationToken);
        }

        private async Task<GraphqlResult<IReadOnlyCollection<TreasuryDto>>> GetTreasury(int portfolioId, string bankAccount, CancellationToken cancellationToken = default)
        {

            var result = new GraphqlResult<IReadOnlyCollection<TreasuryDto>>();
            try
            {
                var listTreasuries = new List<TreasuryDto>();
                var response = await mediator.Send(new GetTreasuryQuery(portfolioId, bankAccount), cancellationToken);

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

                listTreasuries.Add(Treasury);
                result.Data = listTreasuries;

                return result;
            }
            catch (Exception ex)
            {
                result.AddError(new Error("EXCEPTION", ex.Message, ErrorType.Failure));
                return result;
            }
        }

        private async Task<GraphqlResult<IReadOnlyCollection<TreasuryDto>>> GetTreasuries(CancellationToken cancellationToken = default)
        {

            var result = new GraphqlResult<IReadOnlyCollection<TreasuryDto>>();
            try
            {
                var response = await mediator.Send(new GetTreasuriesQuery(), cancellationToken);

                if (!response.IsSuccess || response.Value == null)
                {
                    result.AddError(response.Error);
                    return result;
                }

                var getTreasury = response.Value;

                var Treasury = response.Value.Select(t => new TreasuryDto(
                    t.BankAccount,
                    t.DebitAccount,
                    t.CreditAccount
                )).ToList();

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
