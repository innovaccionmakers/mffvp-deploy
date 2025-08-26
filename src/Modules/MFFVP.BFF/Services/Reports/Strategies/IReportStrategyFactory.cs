using MFFVP.BFF.Services.Reports.Models;

namespace MFFVP.BFF.Services.Reports.Strategies
{
    public interface IReportStrategyFactory
    {
        IReportStrategy GetStrategy(ReportType reportType);
    }
}