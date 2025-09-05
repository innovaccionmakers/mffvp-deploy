using Common.SharedKernel.Application.Reports;
using Microsoft.Extensions.DependencyInjection;
using Reports.Application.BalancesAndMovements;
using Reports.Application.Reports.DailyClosing;
using Reports.Application.TechnicalSheet;

namespace Reports.Application.Strategies
{
    public class ReportStrategyFactory : IReportStrategyFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ReportStrategyFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IReportStrategy GetStrategy(ReportType reportType)
        {
            return reportType switch
            {
                ReportType.DailyClosing => _serviceProvider.GetRequiredService<DailyClosingReport>(),
                ReportType.Balances => _serviceProvider.GetRequiredService<BalancesAndMovementsReport>(),
                ReportType.TechnicalSheet => _serviceProvider.GetRequiredService<TechnicalSheetReport>(),
                _ => throw new ArgumentException($"Tipo de reporte no soportado: {reportType}")
            };
        }
    }
}
