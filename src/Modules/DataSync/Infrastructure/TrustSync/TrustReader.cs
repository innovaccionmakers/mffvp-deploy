using Common.SharedKernel.Core.Primitives;
using DataSync.Application.Abstractions.External.TrustSync;
using DataSync.Application.TrustSync.Dto;
using DataSync.Infrastructure.ConnectionFactory.Interfaces;

namespace DataSync.Infrastructure.TrustSync;

public sealed class TrustReader(ITrustConnectionFactory trustConnectionFactory) : ITrustReader
{
    public async Task<IReadOnlyList<TrustRow>> ReadActiveAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        await using var trustConnection = await trustConnectionFactory.CreateOpenAsync(cancellationToken);

        const string selectSql = @"
            SELECT id, portafolio_id, saldo_total, capital, retencion_contingente
            FROM fideicomisos.fideicomisos
            WHERE estado = @status AND portafolio_id = @p;";

        using var selectCommand = new Npgsql.NpgsqlCommand(selectSql, trustConnection);
        selectCommand.Parameters.AddWithValue("p", portfolioId);
        selectCommand.Parameters.AddWithValue("status", (int)LifecycleStatus.Active);

        var trustRows = new List<TrustRow>();

        await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            trustRows.Add(new TrustRow(
                reader.GetInt64(0),   // id
                reader.GetInt32(1),   // portafolio_id
                closingDate,          // fecha_cierre (proviene del parámetro)
                reader.GetDecimal(2), // saldo_total
                reader.GetDecimal(3), // capital
                reader.GetDecimal(4)  // retencion_contingente
            ));
        }

        return trustRows;
    }
}
