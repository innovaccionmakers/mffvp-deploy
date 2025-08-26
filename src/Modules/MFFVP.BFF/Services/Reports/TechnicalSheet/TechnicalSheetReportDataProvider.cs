using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.Services.Reports.TechnicalSheet.Interfaces;
using Products.Presentation.GraphQL;

namespace MFFVP.BFF.Services.Reports.TechnicalSheet;

public class TechnicalSheetReportDataProvider(
    IProductsExperienceQueries productsExperienceQueries,
    ILogger<TechnicalSheetReportDataProvider> logger) : ITechnicalSheetReportDataProvider
{
    public async IAsyncEnumerable<GraphqlResult<TechnicalSheetReportModel>> GetDataAsync(DateOnly startDate, DateOnly endDate, int portfolioId, CancellationToken cancellationToken)
    {
        var technicalSheets = await productsExperienceQueries.GetTechnicalSheetsByDateRangeAndPortfolio(startDate, endDate, portfolioId, cancellationToken);

        var responseTechnicalSheets = technicalSheets.Select(ts => new TechnicalSheetReportModel(
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
                ts.Participants)).ToList();

        foreach (var ts in responseTechnicalSheets)
        {
            yield return new GraphqlResult<TechnicalSheetReportModel> { Data = ts };
        }
    }

    public IAsyncEnumerable<GraphqlResult<TechnicalSheetReportModel>> GetDataAsync(DateTime processDate, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
