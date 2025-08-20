
using DataSync.Application.Abstractions.External.TrustSync;
using DataSync.Application.TrustSync.Dto;
using DataSync.Infrastructure.ConnectionFactory.Interfaces;

namespace DataSync.Infrastructure.TrustSync;

public sealed class TrustReader(ITrustConnectionFactory factory) : ITrustReader
{
    public async Task<IReadOnlyList<TrustRow>> ReadActiveAsync(int portfolioId, DateTime closingDate, CancellationToken ct)
    {
        await using var conn = await factory.CreateOpenAsync(ct);
        const string sql = @"
            SELECT id, portafolio_id, saldo_total, capital, retencion_contingente
            FROM fideicomisos.fideicomisos
            WHERE estado = true AND portafolio_id = @p;";
        using var cmd = new Npgsql.NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("p", portfolioId);
        var list = new List<TrustRow>();
        await using var rd = await cmd.ExecuteReaderAsync(ct);
        while (await rd.ReadAsync(ct))
        {
            list.Add(new TrustRow(
                rd.GetInt64(0),
                rd.GetInt32(1),
                closingDate,
                rd.GetDecimal(2),
                rd.GetDecimal(3),
                rd.GetDecimal(4)
            ));
        }
        return list;
    }
}
