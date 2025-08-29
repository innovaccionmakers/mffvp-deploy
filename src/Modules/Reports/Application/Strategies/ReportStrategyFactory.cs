using Common.SharedKernel.Application.Reports;
using Microsoft.Extensions.DependencyInjection;
using Reports.Application.BalancesAndMovements;

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
                ReportType.Balances => _serviceProvider.GetRequiredService<BalancesAndMovementsReport>(),
                _ => throw new ArgumentException($"Tipo de reporte no soportado: {reportType}")
            };
        }
    }
}