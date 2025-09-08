using Common.SharedKernel.Application.Reports;

namespace MFFVP.BFF.Services.Reports.Strategies
{
    public interface IReportStrategyFactory
    {
        IReportStrategy GetStrategy(ReportType reportType);
    }
}