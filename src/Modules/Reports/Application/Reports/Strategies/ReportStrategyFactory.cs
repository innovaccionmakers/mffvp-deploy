using Common.SharedKernel.Application.Reports;
using Common.SharedKernel.Application.Reports.Strategies;
using Microsoft.Extensions.DependencyInjection;
using Reports.Application.Reports.BalancesAndMovements;
using Reports.Application.Reports.TransmissionFormat;
using Reports.Application.TechnicalSheet;

namespace Reports.Application.Reports.Strategies
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
                ReportType.TransmissionFormat => _serviceProvider.GetRequiredService<TransmissionFormatReport>(),
                ReportType.Balances => _serviceProvider.GetRequiredService<BalancesAndMovementsReport>(),
                ReportType.TechnicalSheet => _serviceProvider.GetRequiredService<TechnicalSheetReport>(),
                _ => throw new ArgumentException($"Tipo de reporte no soportado: {reportType}")
            };
        }
    }
}
