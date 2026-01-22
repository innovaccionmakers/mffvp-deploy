using Dapper;
using Microsoft.Extensions.Options;
using Reports.Domain.LoadingInfo.Balances;
using Reports.Infrastructure.Common;
using Reports.Infrastructure.Configuration;
using Reports.Infrastructure.ConnectionFactory.Interfaces;
using System.Runtime.CompilerServices;

namespace Reports.Infrastructure.LoadingInfo.Balances;

public sealed class BalanceSheetReadRepository : BaseReadRepository, IBalanceSheetReadRepository
{
    private readonly IReportsDbReadConnectionFactory reportsDbReadConnectionFactory;

    public BalanceSheetReadRepository(
        IReportsDbReadConnectionFactory connectionFactory,
        IOptions<DatabaseTimeoutsOptions> timeoutOptions)
        : base(timeoutOptions)
    {
        reportsDbReadConnectionFactory = connectionFactory;
    }

    public async IAsyncEnumerable<BalanceSheetReadRow> ReadBalancesAsync(
        DateTime closingDateUtc,
        int portfolioId,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var year = closingDateUtc.Year.ToString();
        await using var connection = await reportsDbReadConnectionFactory.CreateOpenAsync(cancellationToken);

        const string validationSql = @"
        SELECT COUNT(*)
        FROM productos.salarios_minimos
        WHERE anio = @Year
    ";

        var count = await connection.ExecuteScalarAsync<int>(
             validationSql,
             new { Year = year },
         commandTimeout: CommandTimeoutSeconds);

        if (count == 0)
        {
            throw new InvalidOperationException(
            $"No se encontró salario mínimo configurado para el año {year}. " +
             $"Debe existir un registro en productos.salarios_minimos antes de procesar el ETL.");
        }

        const string sql = @"
         WITH base_trusts AS (
               SELECT
               f.afiliado_id  AS ""AffiliateId"",
               f.portafolio_id AS ""PortfolioId"",
               f.objetivo_id  AS ""GoalId"",
               rf.fecha_cierre AS ""ClosingDateUtc"",
               f.id  AS ""TrustId"",
               rf.saldo_cierre
               FROM cierre.rendimientos_fideicomisos rf
               JOIN fideicomisos.fideicomisos f
               ON f.id = rf.fideicomiso_id
               WHERE rf.fecha_cierre = @ClosingDateUtc
               AND f.portafolio_id = @PortfolioId
           ),
         base AS (
           SELECT
              ""AffiliateId"",
              ""PortfolioId"",
              ""GoalId"",
              ""ClosingDateUtc"",
              SUM(saldo_cierre) AS ""Balance""
              FROM base_trusts
              GROUP BY
              ""AffiliateId"",
              ""PortfolioId"",
              ""GoalId"",
             ""ClosingDateUtc""
              ),
         people AS (
            SELECT
                aa.id AS afiliado_id,
                per.fecha_nacimiento,
                act.descripcion AS actividad_descripcion
            FROM afiliados.activacion_afiliados aa
            JOIN (SELECT DISTINCT ""AffiliateId"" FROM base) b
                ON b.""AffiliateId"" = aa.id
            LEFT JOIN personas.personas per
            ON per.identificacion = aa.identificacion
            AND per.tipo_documento_uuid = aa.tipo_documento_uuid
                LEFT JOIN personas.actividades_economicas act
            ON act.id = per.actividad_economica_id
        ),
         group_parents AS (
             SELECT id
             FROM operaciones.tipos_operaciones
             WHERE estado = 'A'
             AND atributos_adicionales->>'GrupoLista' IN ('OperacionesClientes', 'NotasContables')
            ),
        allowed_types AS (
             SELECT parent.id, parent.naturaleza
             FROM operaciones.tipos_operaciones parent
             JOIN group_parents gp ON gp.id = parent.id
             WHERE parent.estado = 'A'
             AND parent.naturaleza IN ('I','E')
             UNION ALL
              SELECT child.id, child.naturaleza
              FROM operaciones.tipos_operaciones child
              JOIN group_parents gp ON child.categoria = gp.id
              WHERE child.estado = 'A'
                 AND child.naturaleza IN ('I','E')
          ),
        allowed_types_distinct AS (
             SELECT DISTINCT id, naturaleza
              FROM allowed_types
         ),
        movement_sums AS (
             SELECT
             oc.afiliado_id,
             oc.portafolio_id,
             oc.objetivo_id,
             oc.fecha_proceso,
             COALESCE(SUM(oc.valor) FILTER (WHERE at.naturaleza = 'I'), 0) AS entradas_portafolio,
             COALESCE(SUM(oc.valor) FILTER (WHERE at.naturaleza = 'E'), 0) AS salidas_portafolio
             FROM cierre.operaciones_cliente oc
             JOIN allowed_types_distinct at
             ON at.id = oc.tipo_operaciones_id
             WHERE oc.estado = 1
             AND oc.portafolio_id = @PortfolioId
             AND oc.fecha_proceso = @ClosingDateUtc
             AND EXISTS (
                SELECT 1
                FROM base b
                WHERE b.""AffiliateId""    = oc.afiliado_id
                AND b.""PortfolioId""    = oc.portafolio_id
                AND b.""GoalId""      = oc.objetivo_id
                AND b.""ClosingDateUtc"" = oc.fecha_proceso
             )
             GROUP BY
             oc.afiliado_id,
             oc.portafolio_id,
             oc.objetivo_id,
             oc.fecha_proceso
           ),
           ajuste_type AS (
                SELECT toper.id AS tipo_operaciones_id
                FROM operaciones.tipos_operaciones toper
                WHERE toper.estado = 'A'
                AND toper.nombre = 'Ajuste Rendimientos'
                ORDER BY toper.id
              LIMIT 1
             ),
            withdrawal_adjustments AS (
                SELECT
                bt.""AffiliateId"",
                bt.""PortfolioId"",
                bt.""GoalId"",
                bt.""ClosingDateUtc"",
                COALESCE(SUM(x.ajuste_retiros), 0) AS ajuste_retiros
                FROM base_trusts bt
                CROSS JOIN ajuste_type at
                LEFT JOIN LATERAL (
                SELECT (ofi.valor * -1) AS ajuste_retiros
                FROM operaciones.operaciones_fideicomiso ofi
                WHERE ofi.portafolio_id = bt.""PortfolioId""
                AND ofi.fideicomiso_id = bt.""TrustId""
               AND ofi.fecha_proceso = bt.""ClosingDateUtc""
               AND ofi.tipo_operaciones_id = at.tipo_operaciones_id
               LIMIT 1
             ) x ON true
            GROUP BY
             bt.""AffiliateId"",
             bt.""PortfolioId"",
             bt.""GoalId"",
             bt.""ClosingDateUtc""
             )
             SELECT
             b.""AffiliateId"",
             b.""PortfolioId"",
             b.""GoalId"",
             b.""Balance""::numeric(19,2) AS ""Balance"",
             ROUND((b.""Balance"" / NULLIF(sm.valor, 0))::numeric, 2)::numeric(19,2) AS ""MinimumWages"",
             COALESCE(pf.fondo_id, 0) AS ""FundId"",
             o.fecha_creacion AS ""GoalCreatedAtUtc"",
             DATE_PART('year', AGE(b.""ClosingDateUtc""::date, ppl.fecha_nacimiento::date))::int AS ""Age"",
             CASE WHEN ppl.actividad_descripcion = 'ASALARIADOS' THEN true ELSE false END AS ""IsDependent"",
            COALESCE(ms.entradas_portafolio, 0)::numeric(19,2) AS ""PortfolioEntries"",
            (
                COALESCE(ms.salidas_portafolio, 0)
                + COALESCE(wa.ajuste_retiros, 0)
            )::numeric(19,2) AS ""PortfolioWithdrawals"",
                     b.""ClosingDateUtc"" AS ""ClosingDateUtc""
             FROM base b
            JOIN productos.objetivos o
                 ON o.id = b.""GoalId""
            LEFT JOIN people ppl
            ON ppl.afiliado_id = b.""AffiliateId""
            LEFT JOIN productos.salarios_minimos sm
            ON sm.anio = TO_CHAR(b.""ClosingDateUtc""::date, 'YYYY')
            LEFT JOIN movement_sums ms
            ON ms.afiliado_id   = b.""AffiliateId""
            AND ms.portafolio_id = b.""PortfolioId""
            AND ms.objetivo_id   = b.""GoalId""
            AND ms.fecha_proceso = b.""ClosingDateUtc""
            LEFT JOIN withdrawal_adjustments wa
            ON wa.""AffiliateId""  = b.""AffiliateId""
            AND wa.""PortfolioId"" = b.""PortfolioId""
            AND wa.""GoalId""   = b.""GoalId""
            AND wa.""ClosingDateUtc"" = b.""ClosingDateUtc""
            LEFT JOIN productos.alternativas alt
            ON alt.id = o.alternativa_id
            LEFT JOIN productos.planes_fondo pf
            ON pf.id = alt.planes_fondo_id
            ORDER BY
            b.""PortfolioId"",
            b.""ClosingDateUtc"",
            b.""AffiliateId"",
            b.""GoalId"";
             ";

        var rows = connection.Query<BalanceSheetReadRow>(
            sql,
            new { ClosingDateUtc = closingDateUtc, PortfolioId = portfolioId },
            commandTimeout: CommandTimeoutSeconds,
            buffered: false);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return row;
        }
    }
}