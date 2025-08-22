using DataSync.Application.Abstractions.External.TrustSync;
using DataSync.Infrastructure.ConnectionFactory.Interfaces;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using System.Text.Json;

namespace DataSync.Infrastructure.TrustSync;

public sealed class TrustUnitsUpdater(
    IClosingConnectionFactory closingFactory,
    ITrustConnectionFactory trustFactory,
    ILogger<TrustUnitsUpdater> logger)
    : ITrustUnitsUpdater
{
    public async Task<int> UpdateUnitsAsync(int portfolioId, DateTime closingDate, CancellationToken cancellationToken)
    {
        var trustIds = new List<long>();
        var trustUnits = new List<decimal>();

        // 1) Leer desde Closing 
        await using (var closingConnection = await closingFactory.CreateOpenAsync(cancellationToken))
        {
            const string selectSql = @"
                SELECT fideicomiso_id, unidades
                FROM cierre.rendimientos_fideicomisos
                WHERE portafolio_id = @portfolioId AND fecha_cierre = @closingDate
                ORDER BY fideicomiso_id;"; 

            using var selectCommand = new NpgsqlCommand(selectSql, closingConnection);
            selectCommand.Parameters.AddWithValue("portfolioId", portfolioId);
            selectCommand.Parameters.AddWithValue("closingDate", closingDate.Date);

            await using var reader = await selectCommand.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                trustIds.Add(reader.GetInt64(0));
                trustUnits.Add(reader.GetDecimal(1));
            }
        }

        logger.LogInformation(
            "TrustUnitsUpdater - Portafolio {PortfolioId}, Fecha {ClosingDate}: {Count} registros obtenidos desde cierre.rendimientos_fideicomisos",
            portfolioId, closingDate.Date, trustIds.Count);

        if (trustIds.Count > 0)
        {
            var fetchedData = trustIds
                .Select((id, index) => new { Id = id, Units = trustUnits[index] })
                .ToList();
            logger.LogInformation(
                "TrustUnitsUpdater - Datos obtenidos: {FetchedData}",
                JsonSerializer.Serialize(fetchedData, new JsonSerializerOptions { WriteIndented = true }));
        }

        if (trustIds.Count == 0) return 0;

        var unitsById = trustIds
            .Select((id, i) => new { id, u = trustUnits[i] })
            .ToDictionary(x => x.id, x => x.u);

        await using var trustConnection = await trustFactory.CreateOpenAsync(cancellationToken);
        await using var transaction = await trustConnection.BeginTransactionAsync(cancellationToken);

        // 2) Diagnóstico previo: ver qué va a actualizarse (antes del UPDATE)
        const string previewSql = @"
            WITH src AS (
              SELECT fideicomiso, unidades
              FROM unnest(@ids::bigint[], @units::numeric(38,16)[]) AS s(fideicomiso, unidades)
            )
            SELECT f.id               AS target_id,
                   f.unidades_totales AS before_units,
                   s.unidades         AS new_units
            FROM fideicomisos.fideicomisos f
            JOIN src s ON f.id = s.fideicomiso
            ORDER BY target_id;";
        using (var previewCmd = new NpgsqlCommand(previewSql, trustConnection, transaction))
        {
            previewCmd.Parameters.AddWithValue("ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint, trustIds.ToArray());
            previewCmd.Parameters.AddWithValue("units", NpgsqlDbType.Array | NpgsqlDbType.Numeric, trustUnits.ToArray());

            var previewList = new List<object>();
            await using var previewReader = await previewCmd.ExecuteReaderAsync(cancellationToken);
            while (await previewReader.ReadAsync(cancellationToken))
            {
                previewList.Add(new
                {
                    Id = previewReader.GetInt64(0),
                    Before = previewReader.GetDecimal(1),
                    New = previewReader.GetDecimal(2)
                });
            }

            logger.LogInformation(
                "TrustUnitsUpdater - PREVIEW cambios en fideicomisos: {Preview}",
                JsonSerializer.Serialize(previewList, new JsonSerializerOptions { WriteIndented = true }));
        }

        // 3) UPDATE batch (zip arrays) con retorno extendido
        const string updateSql = @"
            WITH src AS (
              SELECT fideicomiso, unidades
              FROM unnest(@ids::bigint[], @units::numeric(38,16)[]) AS s(fideicomiso, unidades)
            )
            UPDATE fideicomisos.fideicomisos f
               SET unidades_totales = s.unidades
            FROM src s
            WHERE f.id = s.fideicomiso
            RETURNING f.id, s.unidades, f.unidades_totales;";

        var updatedIds = new List<long>();
        var updatedEcho = new List<object>();

        using (var updateCommand = new NpgsqlCommand(updateSql, trustConnection, transaction))
        {
            updateCommand.Parameters.AddWithValue("ids", NpgsqlDbType.Array | NpgsqlDbType.Bigint, trustIds.ToArray());
            updateCommand.Parameters.AddWithValue("units", NpgsqlDbType.Array | NpgsqlDbType.Numeric, trustUnits.ToArray());

            await using var updatedReader = await updateCommand.ExecuteReaderAsync(cancellationToken);
            while (await updatedReader.ReadAsync(cancellationToken))
            {
                var id = updatedReader.GetInt64(0);
                var newUnits = updatedReader.GetDecimal(1);
                var afterUnits = updatedReader.GetDecimal(2);
                updatedIds.Add(id);
                updatedEcho.Add(new { Id = id, New = newUnits, After = afterUnits });
            }
        }

        logger.LogInformation(
            "TrustUnitsUpdater - RESULT batch update: {Details}",
            JsonSerializer.Serialize(updatedEcho, new JsonSerializerOptions { WriteIndented = true }));

        // 4) Fallback: si faltaron filas, intenta por id (1x1) para aislar causa
        var updatedCount = updatedIds.Count;
        if (updatedCount != trustIds.Count)
        {
            var missingIds = trustIds.Except(updatedIds).Distinct().OrderBy(x => x).ToList();

            logger.LogWarning(
                "TrustUnitsUpdater - Faltan por actualizar {Missing} de {Total}. Intentando fallback por ID...",
                missingIds.Count, trustIds.Count);

            const string updateOneSql = @"
                UPDATE fideicomisos.fideicomisos
                   SET unidades_totales = @u
                 WHERE id = @id
                RETURNING id, unidades_totales;";
            using var updateOne = new NpgsqlCommand(updateOneSql, trustConnection, transaction);
            var pId = updateOne.Parameters.Add("id", NpgsqlDbType.Bigint);
            var pU = updateOne.Parameters.Add("u", NpgsqlDbType.Numeric);

            foreach (var id in missingIds)
            {
                pId.Value = id;
                pU.Value = unitsById[id];

                await using var r = await updateOne.ExecuteReaderAsync(cancellationToken);
                if (await r.ReadAsync(cancellationToken))
                {
                    var after = r.GetDecimal(1);
                    updatedIds.Add(id);
                    logger.LogInformation("Fallback OK -> id={Id}, unidades_totales={After}", id, after);
                }
                else
                {
                    // No tocó fila: o no existe el id, o hay RLS/permiso/tenancy, o no es visible
                    logger.LogWarning("Fallback NO actualizó -> id={Id}, units={Units}", id, unitsById[id]);
                }
            }

            updatedCount = updatedIds.Distinct().Count();
        }

        await transaction.CommitAsync(cancellationToken);

        // 5) Post-logs finales
        if (updatedCount != trustIds.Count)
        {
            var notUpdatedIds = trustIds.Except(updatedIds).OrderBy(x => x).ToList();
            var notUpdatedData = notUpdatedIds.Select(id => new { Id = id, Units = unitsById[id] }).ToList();

            logger.LogWarning(
                "TrustUnitsUpdater - Portafolio {PortfolioId}, Fecha {ClosingDate}: obtenidos={Fetched}, actualizados={Updated}.",
                portfolioId, closingDate.Date, trustIds.Count, updatedCount);

            logger.LogWarning(
                "TrustUnitsUpdater - IDs no actualizados (con unidades): {NotUpdated}",
                JsonSerializer.Serialize(notUpdatedData, new JsonSerializerOptions { WriteIndented = true }));
        }
        else
        {
            logger.LogInformation(
                "TrustUnitsUpdater - Portafolio {PortfolioId}, Fecha {ClosingDate}: {Updated} registros actualizados en fideicomisos.fideicomisos",
                portfolioId, closingDate.Date, updatedCount);
        }

        return updatedCount;
    }
}
