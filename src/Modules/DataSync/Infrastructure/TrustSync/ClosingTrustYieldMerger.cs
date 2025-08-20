using DataSync.Application.Abstractions.External.TrustSync;
using DataSync.Application.TrustSync.Dto;
using DataSync.Infrastructure.ConnectionFactory.Interfaces;

namespace DataSync.Infrastructure.TrustSync;

public sealed class ClosingTrustYieldMerger(IClosingConnectionFactory factory)
    : IClosingTrustYieldMerger
{
    public async Task<int> MergeAsync(IReadOnlyList<TrustRow> rows, CancellationToken ct)
    {
        var fids = rows.Select(r => r.TrustId).ToArray();
        var ports = rows.Select(r => r.PortfolioId).ToArray();
        var dates = rows.Select(r => r.ClosingDate).ToArray();
        var bal = rows.Select(r => r.PreClosingBalance).ToArray();
        var caps = rows.Select(r => r.Capital).ToArray();
        var rc = rows.Select(r => r.ContingentRetention).ToArray();

        await using var conn = await factory.CreateOpenAsync(ct);
        await using var tx = await conn.BeginTransactionAsync(ct);

        const string mergeSql = @"
            MERGE INTO cierre.rendimientos_fideicomisos AS t
            USING (
              SELECT
                unnest(@fids::bigint[])     AS fideicomiso,
                unnest(@ports::int[])       AS portafolio,
                unnest(@dates::date[])      AS fecha_cierre,
                unnest(@bal::numeric[])     AS saldo_precierre,
                unnest(@caps::numeric[])    AS capital,
                unnest(@rc::numeric[])      AS retencion_contingente
            ) AS s
            ON (t.portafolio_id = s.portafolio
                AND t.fideicomiso_id = s.fideicomiso
                AND t.fecha_cierre = s.fecha_cierre)
            WHEN MATCHED THEN UPDATE SET
                saldo_precierre       = s.saldo_precierre,
                capital               = s.capital,
                retencion_contingente = s.retencion_contingente,
                fecha_proceso         = now()
            WHEN NOT MATCHED THEN INSERT
                (fideicomiso_id, portafolio_id, fecha_cierre,
                 saldo_precierre, capital, retencion_contingente,
                 fecha_proceso,
                 participacion, unidades, rendimientos, saldo_cierre,
                 ingresos, gastos, comisiones, costo, retencion_rendimiento)
            VALUES
                (s.fideicomiso, s.portafolio, s.fecha_cierre,
                 s.saldo_precierre, s.capital, s.retencion_contingente,
                 now(),
                 0, 0, 0, 0,
                 0, 0, 0, 0, 0);";

        using var cmd = new Npgsql.NpgsqlCommand(mergeSql, conn, tx);
        cmd.Parameters.AddWithValue("fids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint, fids);
        cmd.Parameters.AddWithValue("ports", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, ports);
        cmd.Parameters.AddWithValue("dates", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Date, dates);
        cmd.Parameters.AddWithValue("bal", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Numeric, bal);
        cmd.Parameters.AddWithValue("caps", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Numeric, caps);
        cmd.Parameters.AddWithValue("rc", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Numeric, rc);

        var affected = await cmd.ExecuteNonQueryAsync(ct);
        await tx.CommitAsync(ct);
        return affected; // UPDATE + INSERT totales
    }
}
