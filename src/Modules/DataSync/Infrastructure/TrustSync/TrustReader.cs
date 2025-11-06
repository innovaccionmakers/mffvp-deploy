using System.Data.Common;
using Common.SharedKernel.Core.Primitives;
using DataSync.Application.Abstractions.External.TrustSync;
using DataSync.Application.TrustSync.Dto;
using DataSync.Infrastructure.ConnectionFactory.Interfaces;
using Npgsql;
using NpgsqlTypes;

namespace DataSync.Infrastructure.TrustSync;

public class TrustReader(ITrustConnectionFactory trustConnectionFactory) : ITrustReader
{
    public async Task<IReadOnlyList<TrustRow>> ReadActiveAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        await using var trustConnection = await trustConnectionFactory.CreateOpenAsync(cancellationToken);

        const string selectSql = """
            SELECT id, portafolio_id, saldo_total, capital, retencion_contingente
            FROM fideicomisos.fideicomisos
            WHERE estado = ANY(@statuses) AND portafolio_id = @p AND fecha_actualizacion = @closingDate;
            """;

        var truncatedClosingDate = closingDate.Date;

        await using var selectCommand = CreateCommand(trustConnection, selectSql);
        selectCommand.Parameters.AddWithValue("p", portfolioId);
        selectCommand.Parameters.AddWithValue("statuses", NpgsqlDbType.Array | NpgsqlDbType.Integer, new[]
        {
            (int)LifecycleStatus.Active,
            (int)LifecycleStatus.AnnulledByDebitNote
        });
        selectCommand.Parameters.AddWithValue("closingDate", NpgsqlDbType.Date, truncatedClosingDate);

        var trustRows = new List<TrustRow>();

        await using var reader = await ExecuteReaderAsync(selectCommand, cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            trustRows.Add(new TrustRow(
                reader.GetInt64(0),   // id
                reader.GetInt32(1),   // portafolio_id
                truncatedClosingDate, // fecha_cierre (proviene del parámetro)
                reader.GetDecimal(2), // saldo_total
                reader.GetDecimal(3), // capital
                reader.GetDecimal(4)  // retencion_contingente
            ));
        }

        return trustRows;
    }

    protected virtual NpgsqlCommand CreateCommand(NpgsqlConnection connection, string commandText)
    {
        var command = connection.CreateCommand();
        command.CommandText = commandText;
        return command;
    }

    protected virtual async Task<DbDataReader> ExecuteReaderAsync(NpgsqlCommand command, CancellationToken cancellationToken)
        => await command.ExecuteReaderAsync(cancellationToken);
}
