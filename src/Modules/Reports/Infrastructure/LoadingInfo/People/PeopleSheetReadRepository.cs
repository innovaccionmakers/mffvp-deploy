using Dapper;
using Microsoft.Extensions.Options;
using Reports.Domain.LoadingInfo.People;
using Reports.Infrastructure.Common;
using Reports.Infrastructure.Configuration;
using Reports.Infrastructure.ConnectionFactory.Interfaces;

namespace Reports.Infrastructure.LoadingInfo.People;

public sealed class PeopleSheetReadRepository : BaseReadRepository, IPeopleSheetReadRepository
{
    private readonly IReportsDbReadConnectionFactory reportsDbReadConnectionFactory;

    public PeopleSheetReadRepository(IReportsDbReadConnectionFactory connectionFactory,
        IOptions<DatabaseTimeoutsOptions> timeoutOptions)
        : base(timeoutOptions)
    {
        this.reportsDbReadConnectionFactory = connectionFactory;
    }

    public async IAsyncEnumerable<PeopleSheetReadRow> ReadPeopleAsync(
        long? lastRowVersion,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // Consulta incremental basada en row_version de personas.personas
        var sql = @"
          SELECT
          aa.id AS ""MemberId"",
          tipo_doc.nombre AS ""IdentificationType"",
          tipo_doc.codigo_homologacion AS ""IdentificationTypeHomologated"",
          per.identificacion AS ""Identification"",
          per.nombre_completo AS ""FullName"",
          (per.fecha_nacimiento AT TIME ZONE 'UTC')::timestamp AS ""Birthday"",
          sexo.nombre AS ""Gender""
         FROM afiliados.activacion_afiliados aa
         JOIN personas.personas per
            ON per.identificacion = aa.identificacion
            AND per.tipo_documento_uuid = aa.tipo_documento_uuid
         JOIN personas.parametros_configuracion tipo_doc
            ON tipo_doc.uuid = per.tipo_documento_uuid
         JOIN personas.parametros_configuracion sexo
             ON sexo.tipo = 'Sexo' AND sexo.id = per.sexo_id
         WHERE (@LastRowVersion IS NULL OR per.row_version > @LastRowVersion);
         ";

        await using var connection = await reportsDbReadConnectionFactory.CreateOpenAsync(cancellationToken);

        var rows = connection.Query<PeopleSheetReadRow>(
                         sql,
                         param: new { LastRowVersion = lastRowVersion },
                         commandTimeout: CommandTimeoutSeconds,
                         buffered: false);

        foreach (var row in rows)
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return row;
        }
    }
}
