using Common.SharedKernel.Presentation.Results;
using MFFVP.BFF.Services.Reports.DepositsReport.Interfaces;

namespace MFFVP.BFF.Services.Reports.TechnicalSheet.Interfaces;

public interface ITechnicalSheetReportDataProvider : IReportDataProvider<TechnicalSheetReportModel>
{
    IAsyncEnumerable<GraphqlResult<TechnicalSheetReportModel>> GetDataAsync(DateOnly startDate, DateOnly endDate, int portfolioId, CancellationToken cancellationToken);
}
