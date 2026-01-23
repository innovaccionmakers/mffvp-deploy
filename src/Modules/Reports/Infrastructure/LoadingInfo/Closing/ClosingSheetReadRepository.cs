using Dapper;
using Microsoft.Extensions.Options;
using Reports.Domain.LoadingInfo.Closing;
using Reports.Infrastructure.Common;
using Reports.Infrastructure.Configuration;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using System.Runtime.CompilerServices;

namespace Reports.Infrastructure.LoadingInfo.Closing;

public sealed class ClosingSheetReadRepository : BaseReadRepository, IClosingSheetReadRepository
{
    private readonly IReportsDbReadConnectionFactory reportsDbReadConnectionFactory;

    public ClosingSheetReadRepository(
        IReportsDbReadConnectionFactory connectionFactory,
        IOptions<DatabaseTimeoutsOptions> timeoutOptions)
        : base(timeoutOptions)
    {
        reportsDbReadConnectionFactory = connectionFactory;
    }

    public async IAsyncEnumerable<ClosingSheetReadRow> ReadClosingAsync(
        DateTime closingDateUtc,
        int portfolioId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const string sql = @"
       SELECT
       vp.portafolio_id AS ""PortfolioId"",
       (vp.fecha_cierre AT TIME ZONE 'UTC')::timestamp AS ""ClosingDate"",
       vp.operaciones_entrada AS ""Contributions"",
       vp.operaciones_salida AS ""Withdrawals"",
       r.ingresos AS ""GrossPandL"",
       r.gastos AS ""Expenses"",
       r.comisiones AS ""DailyFee"",
       r.costos AS ""DailyCost"",
       r.rendimientos_abonar AS ""YieldsToCredit"",
       vp.rendimiento_bruto_unidad AS ""GrossYieldPerUnit"",
       vp.costo_unidad AS ""CostPerUnit"",
       vp.valor_unidad AS ""UnitValue"",
       vp.unidades AS ""Units"",
       vp.valor AS ""PortfolioValue"",
       COALESCE(p.participants, 0) AS ""Participants"",
       c.regla_calculo AS ""PortfolioFeePercentage"",
       pf.fondo_id AS ""FundId""
      FROM cierre.valoracion_portafolio vp
      JOIN cierre.rendimientos r
       ON r.portafolio_id = vp.portafolio_id
       AND r.fecha_cierre = vp.fecha_cierre
        AND r.cerrado = vp.cerrado
      LEFT JOIN LATERAL (
        SELECT COUNT(DISTINCT f.afiliado_id) AS participants
        FROM cierre.rendimientos_fideicomisos rf
        JOIN fideicomisos.fideicomisos f ON f.id = rf.fideicomiso_id
        WHERE rf.portafolio_id = vp.portafolio_id
        AND rf.fecha_cierre = vp.fecha_cierre
         AND rf.saldo_cierre > 0
       ) p ON true
       LEFT JOIN productos.alternativas_portafolios ap
        ON ap.""PortfolioId"" = vp.portafolio_id
        AND ap.estado = 'A'
       LEFT JOIN productos.alternativas a
         ON a.id = ap.""AlternativeId""
       LEFT JOIN productos.planes_fondo pf
        ON pf.id = a.planes_fondo_id
       LEFT JOIN LATERAL (
        SELECT 
         NULLIF(c2.regla_calculo, '')::numeric(3,2) AS regla_calculo
        FROM productos.comisiones c2
        WHERE c2.portfolio_id = vp.portafolio_id
          AND c2.estado = 'A'
          AND c2.concepto = 'Administración'
        ORDER BY c2.id ASC
         LIMIT 1
        ) c ON true
        WHERE vp.fecha_cierre = @ClosingDateUtc
        AND vp.portafolio_id = @PortfolioId
        AND vp.cerrado = true;
        ";

        await using var connection = await reportsDbReadConnectionFactory.CreateOpenAsync(cancellationToken);

        var rows = connection.Query<ClosingSheetReadRow>(
               sql,
               new
               {
                   ClosingDateUtc = closingDateUtc,
                   PortfolioId = portfolioId
               },
               commandTimeout: CommandTimeoutSeconds,
               buffered: false);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return row;
        }
    }
}
