using DataSync.Application.Abstractions.External.TrustSync;
using DataSync.Application.TrustSync.Dto;
using DataSync.Infrastructure.ConnectionFactory.Interfaces;

namespace DataSync.Infrastructure.TrustSync;

public sealed class ClosingTrustYieldMerger(IClosingConnectionFactory closingConnectionFactory)
    : IClosingTrustYieldMerger
{
    public async Task<int> MergeAsync(IReadOnlyList<TrustRow> trustRows, CancellationToken cancellationToken)
    {
        var trustIds = trustRows.Select(r => r.TrustId).ToArray();
        var portfolioIds = trustRows.Select(r => r.PortfolioId).ToArray();
        var closingDates = trustRows.Select(r => r.ClosingDate).ToArray();
        var preClosingBalances = trustRows.Select(r => r.PreClosingBalance).ToArray();
        var capitals = trustRows.Select(r => r.Capital).ToArray();
        var contingentRetentions = trustRows.Select(r => r.ContingentRetention).ToArray();

        await using var closingConnection = await closingConnectionFactory.CreateOpenAsync(cancellationToken);
        await using var transaction = await closingConnection.BeginTransactionAsync(cancellationToken);

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

        using var mergeCommand = new Npgsql.NpgsqlCommand(mergeSql, closingConnection, transaction);
        mergeCommand.Parameters.AddWithValue("fids", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint, trustIds);
        mergeCommand.Parameters.AddWithValue("ports", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Integer, portfolioIds);
        mergeCommand.Parameters.AddWithValue("dates", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Date, closingDates);
        mergeCommand.Parameters.AddWithValue("bal", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Numeric, preClosingBalances);
        mergeCommand.Parameters.AddWithValue("caps", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Numeric, capitals);
        mergeCommand.Parameters.AddWithValue("rc", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Numeric, contingentRetentions);

        var affectedRows = await mergeCommand.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return affectedRows; // UPDATE + INSERT totales
    }
}
