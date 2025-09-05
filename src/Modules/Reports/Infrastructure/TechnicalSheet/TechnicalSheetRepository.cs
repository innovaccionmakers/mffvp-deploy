namespace Reports.Infrastructure.TechnicalSheet;

using Dapper;
using Microsoft.Extensions.Options;
using Reports.Domain.TechnicalSheet;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using Reports.Infrastructure.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

internal class TechnicalSheetRepository : ITechnicalSheetRepository
{
    private readonly IReportsDbConnectionFactory _dbConnectionFactory;
    private readonly string _productsSchema;

    public TechnicalSheetRepository(IReportsDbConnectionFactory dbConnectionFactory, IOptions<ReportsOptions> options)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _productsSchema = string.IsNullOrWhiteSpace(options.Value.ProductsSchema)
            ? "productos"
            : options.Value.ProductsSchema!;
    }

    public async Task<IEnumerable<TechnicalSheet>> GetByDateRangeAndPortfolioAsync(
        DateOnly startDate,
        DateOnly endDate,
        int portfolioId,
        CancellationToken cancellationToken = default)
    {
        var startDateTime = startDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var endDateTime = endDate.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var sql = $@"
        SELECT 
            ""fecha""                    AS ""Date"",
            ""aportes""                  AS ""Contributions"",
            ""retiros""                  AS ""Withdrawals"",
            ""pyg_bruto""                AS ""GrossPnl"",
            ""gastos""                   AS ""Expenses"",
            ""comision_dia""             AS ""DailyCommission"",
            ""costo_dia""                AS ""DailyCost"",
            ""rendimientos_abonados""    AS ""CreditedYields"",
            ""rendimiento_bruto_unidad"" AS ""GrossUnitYield"",
            ""costo_unidad""             AS ""UnitCost"",
            ""valor_unidad""             AS ""UnitValue"",
            ""unidades""                 AS ""Units"",
            ""valor_portafolio""         AS ""PortfolioValue"",
            ""participes""               AS ""Participants""
        FROM ""{_productsSchema}"".""ficha_tecnica""
        WHERE ""portafolio_id"" = @PortfolioId
          AND ""fecha"" BETWEEN @StartDate AND @EndDate
        ORDER BY ""fecha"";";

        await using var connection = await _dbConnectionFactory.CreateOpenAsync(cancellationToken);

        return await connection.QueryAsync<TechnicalSheet>(
            new CommandDefinition(
                sql,
                new { PortfolioId = portfolioId, StartDate = startDateTime, EndDate = endDateTime },
                cancellationToken: cancellationToken
            )
        );
    }
}
