using DataSync.Application.Abstractions.External.TrustSync;
using DataSync.Infrastructure.ConnectionFactory.Interfaces;

namespace DataSync.Infrastructure.TrustSync;

public sealed class TrustUnitsUpdater(
    IClosingConnectionFactory closingFactory,
    ITrustConnectionFactory trustFactory)
    : ITrustUnitsUpdater
{
    public async Task<int> UpdateUnitsAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        // leer desde Closing
        var ids = new List<long>();
        var units = new List<decimal>();
        await using (var c = await closingFactory.CreateOpenAsync(ct))
        {
            const string sql = @"
                SELECT fideicomiso_id, unidades
                FROM cierre.rendimientos_fideicomisos
                WHERE portafolio_id = @p AND fecha_cierre = @d";
            using var cmd = new Npgsql.NpgsqlCommand(sql, c);
            cmd.Parameters.AddWithValue("p", portfolioId);
            cmd.Parameters.AddWithValue("d", closingDate.Date);
            await using var rd = await cmd.ExecuteReaderAsync(ct);
            while (await rd.ReadAsync(ct)) { ids.Add(rd.GetInt64(0)); units.Add(rd.GetDecimal(1)); }
        }
        if (ids.Count == 0) return 0;


        await using var t = await trustFactory.CreateOpenAsync(ct);
        await using var tx = await t.BeginTransactionAsync(ct);

        const string upd = @"
            WITH src AS (
              SELECT unnest(@ids::bigint[]) AS fideicomiso,
                     unnest(@u::numeric[])  AS unidades
            )
            UPDATE fideicomisos.fideicomisos f
            SET unidades_totales = s.unidades
            FROM src s
            WHERE f.id = s.fideicomiso;";
        using var ucmd = new Npgsql.NpgsqlCommand(upd, t, tx);
        ucmd.Parameters.AddWithValue("ids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint, ids.ToArray());
        ucmd.Parameters.AddWithValue("u", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Numeric, units.ToArray());

        var updated = await ucmd.ExecuteNonQueryAsync(ct);
        await tx.CommitAsync(ct);
        return updated;
    }
}