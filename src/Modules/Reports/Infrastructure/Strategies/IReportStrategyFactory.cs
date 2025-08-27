
using Reports.Infrastructure.Models;

namespace Reports.Infrastructure.Strategies
{
    public interface IReportStrategyFactory
    {
        IReportStrategy GetStrategy(ReportType reportType);
    }
}