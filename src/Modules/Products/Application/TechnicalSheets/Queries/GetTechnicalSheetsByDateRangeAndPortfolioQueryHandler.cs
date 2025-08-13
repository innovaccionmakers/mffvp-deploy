using Common.SharedKernel.Application.Messaging;
using Common.SharedKernel.Domain;
using Products.Domain.TechnicalSheets;
using Products.Integrations.TechnicalSheets.Queries;
using Products.Integrations.TechnicalSheets.Response;

namespace Products.Application.TechnicalSheets.Queries;

internal sealed class GetTechnicalSheetsByDateRangeAndPortfolioQueryHandler(
    ITechnicalSheetRepository repository)
    : IQueryHandler<GetTechnicalSheetsByDateRangeAndPortfolioQuery, IReadOnlyCollection<TechnicalSheetResponse>>
{
    public async Task<Result<IReadOnlyCollection<TechnicalSheetResponse>>> Handle(
        GetTechnicalSheetsByDateRangeAndPortfolioQuery request,
        CancellationToken cancellationToken)
    {
        var technicalSheets = await repository.GetByDateRangeAndPortfolioAsync(
            request.StartDate,
            request.EndDate,
            request.PortfolioId,
            cancellationToken);

        var response = technicalSheets
            .Select(ts => new TechnicalSheetResponse(
                ts.TechnicalSheetId,
                ts.PortfolioId,
                ts.Date,
                ts.Contributions,
                ts.Withdrawals,
                ts.GrossPnl,
                ts.Expenses,
                ts.DailyCommission,
                ts.DailyCost,
                ts.CreditedYields,
                ts.GrossUnitYield,
                ts.UnitCost,
                ts.UnitValue,
                ts.Units,
                ts.PortfolioValue,
                ts.Participants))
            .ToList();

        return Result.Success<IReadOnlyCollection<TechnicalSheetResponse>>(response);
    }
}
