using System.Text;
using Common.SharedKernel.Application.Reports;
using Reports.Application.Reports.Common.Strategies;
using Reports.Domain.DailyClosing;
using Reports.Domain.DailyClosing.Records;

namespace Reports.Application.Reports.DailyClosing;

public interface IDailyClosingReportBuilder
{
    string Build(Rt1Header rt1Header, Rt2Header rt2Header, DailyReportData data, DateTime generationDate);
}

public class DailyClosingReportBuilder(IReportStrategyBuilder strategyBuilder) : IDailyClosingReportBuilder
{
    public string Build(Rt1Header rt1Header, Rt2Header rt2Header, DailyReportData data, DateTime generationDate)
    {
        var records = new List<string>();
        var recordNumber = 1;

        // RT2
        recordNumber++;
        var rt2 = new Rt2Record(rt2Header.PortfolioCode).ToLine(recordNumber);
        records.Add(rt2);

        // RT4 (delegado a la estrategia del reporte)
        var strategy = strategyBuilder.Build(ReportType.DailyClosing);
        strategy.Generate(data, records, ref recordNumber);

        // RT5
        recordNumber++;
        var rt5 = new Rt5Record().ToLine(recordNumber);
        records.Add(rt5);

        // RT1 (al inicio con el total real de registros)
        var rt1 = new Rt1Record(rt1Header, generationDate, recordNumber).ToLine();
        records.Insert(0, rt1);

        var builder = new StringBuilder();
        foreach (var line in records)
        {
            builder.AppendLine(line);
        }

        return builder.ToString();
    }
}

