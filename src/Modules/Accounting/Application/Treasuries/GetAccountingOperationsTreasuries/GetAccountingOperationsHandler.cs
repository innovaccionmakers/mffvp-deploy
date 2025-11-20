using Accounting.Domain.Treasuries;
using Accounting.Integrations.Treasuries.GetAccountingOperationsTreasuries;
using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;

namespace Accounting.Application.Treasuries.GetAccountingOperationsTreasuries
{
    internal class GetAccountingOperationsHandler(
    ITreasuryRepository treasuryRepository) : IQueryHandler<GetAccountingOperationsTreasuriesQuery, IReadOnlyCollection<GetAccountingOperationsTreasuriesResponse>>
    {
        public async Task<Result<IReadOnlyCollection<GetAccountingOperationsTreasuriesResponse>>> Handle(
            GetAccountingOperationsTreasuriesQuery query,
            CancellationToken cancellationToken)
        {
            var treasury = await treasuryRepository.GetAccountingOperationsTreasuriesAsync(query.PortfolioIds, query.CollectionAccount, cancellationToken);

            var response = treasury
            .Select(t => new GetAccountingOperationsTreasuriesResponse(
                t.PortfolioId,
                t.DebitAccount,
                t.CreditAccount))
            .ToList();

            return Result.Success<IReadOnlyCollection<GetAccountingOperationsTreasuriesResponse>>(response);
        }
    }
}
