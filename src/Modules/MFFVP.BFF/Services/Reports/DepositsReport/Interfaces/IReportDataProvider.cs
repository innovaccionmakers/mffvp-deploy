using MFFVP.BFF.Services.Reports.Models;

namespace MFFVP.BFF.Services.Reports.DepositsReport.Interfaces
{
    public interface IReportDataProvider<T> where T : ReportModelBase
    {
        IAsyncEnumerable<T> GetDataAsync(DateTime processDate, CancellationToken cancellationToken);
    }
}
